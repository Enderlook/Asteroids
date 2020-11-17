using Asteroids.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_GlobalMementoManager)]
    public class GlobalMementoManager : MonoBehaviour
    {
        private static GlobalMementoManager instance;

        private const float expirationTime = 6;
        private const float rewindTime = 4; // Rewind less than stored to prevent subtle bugs
        private static readonly int aproximateStoredmementos = Mathf.CeilToInt(50 * expirationTime);

        private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        public static bool IsRewinding => instance.stopAt > Time.fixedTime;

        private List<IMementoManager> managers = new List<IMementoManager>();
        private float stopAt;
        private float speed;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"Can only have a single instance of {nameof(GlobalMementoManager)}.");
                Destroy(this);
            }
            else
                instance = this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            /*
             * It may look crazy to store a memento per object per fixed update (50 fixed updates per second), but actually it's not so crazy:
             * 
             * Each enemy memento is composed of (bool, Vector3, float, Vector2, float, Sprite). That is (1 + 3 + 1 + 2 + (1 + 1/2 + 1/2)) 40/48 bytes depending CPU arch.
             * A level can't spawn more than 12 enemies.
             * Imagine that all those enemies are big. Now you have 12 big enemies.
             * All big enemies were killed an split into 2 medium enemies each one. Now you have 24 medium enemies.
             * All medium enemies were killed an split into 2 small enemies each one. Now you have 48 small enemies.
             * That means there can be up to 12 + 24 + 48 = 84 instances of enemies on memory.
             * 
             * A player memento is (Vector3, float, Vector2, float). That is 28 bytes.
             * An ability memento is (float). That is 4 bytes.
             * A laser memento is (float). That is 4 bytes.
             * An enemy spawner memento is (int). That is 4 bytes. 
             * 
             * A bullet memento is (bool, Vector3, float, Vector2, float). That is 29 bytes.
             * Bullets are shooted each 0.35 seconds with a force of 10 and mass of 1 and the boundary is at 10 offset,
             * and largest distances are diagonal.
             * So there can be (1 / 0.35) * (((10^2 + 10^2) ^ 0.5) / (10 / 1)) = 4,04 = 5 instance on memory.
             * 
             * The total size of mementos per collection is 84 * 48 + 28 + 4 + 4 + 4 + 4 * 29 = 4,188 bytes = 4.08 kb.
             * 
             * The total size of mementos in memory is 4.188 * 50 * 5 = 1,047,000 bytes = 1,022.46 kb, not an huge deal.
             * 
             * In terms of allocations, 4,188 * 50 = 209,400 bytes/s = 204.49 kb/s is an huge deal.
             * However, our mementos are value types, so we are not allocating/deallocating 204.49 kb/s.
             * Instead, value types are inlined in the underlying array of the queue (which is implemented as a circular array),
             * so actually, allocations only happens during the first 5 seconds until queues reaches their maximum size and no longer resize.
             */

            if (IsRewinding)
            {
                float delta = Time.fixedDeltaTime * speed;
                foreach (IMementoManager manager in managers)
                    manager.UpdateRewind(delta);
            }
            else
            {
                if (!Physics.autoSimulation)
                {
                    Physics.autoSimulation = true;
                    EventManager.Raise(new StopRewindEvent());
                }

                foreach (IMementoManager manager in managers)
                    manager.Store();
            }
        }

        public static void Rewind(float duration)
        {
            instance.stopAt = Time.fixedTime + expirationTime;

            Physics.autoSimulation = false;
            EventManager.Raise(new StartRewindEvent());
            instance.speed = rewindTime / duration;
            foreach (IMementoManager manager in instance.managers)
                manager.StartRewind();
        }

        public static void Subscribe<T>(Func<T> onStore, Action<T?> onRewind, Func<T, T, float, T> interpolate) where T : struct
        {
            // We don't require to remove callbacks never because this object and its originators are stop being used at the same time

            MementoManager<T> manager = new MementoManager<T>(onStore, onRewind, interpolate);
            instance.managers.Add(manager);
        }

        private interface IMementoManager
        {
            void Store();

            void StartRewind();

            // We don't use Coroutines because they produced an insane bottleneck,
            // this is extremely more performant and doesn't allocate
            void UpdateRewind(float deltaTime);
        }

        public class MementoManager<T> : IMementoManager where T : struct
        {
            // We store Memento objects in an stongly typed fashion to avoid boxing and so reduce GC pressure

            private readonly Queue<(T memento, float expiration)> queue = new Queue<(T memento, float expiration)>(aproximateStoredmementos);

            // Profiling shows that this optimization reduces allocation by a factor of x4 when stack is smaller than queue
            // and execution time by x2 when calling StartRewind()
            private readonly Stack<(T memento, float delta)> stack = new Stack<(T memento, float delta)>(aproximateStoredmementos);
            private Func<T> onStore;
            private Func<T, T, float, T> interpolate;
            private Action<T?> onRewind;

            public MementoManager(Func<T> onStore, Action<T?> onRewind, Func<T, T, float, T> interpolate)
            {
                this.onRewind = onRewind;
                this.onStore = onStore;
                this.interpolate = interpolate;
            }

            void IMementoManager.Store() => Store();

            private void Store()
            {
                T memento = onStore();
                queue.Enqueue((memento, Time.fixedTime + expirationTime));
                while (queue.TryPeek(out (T _, float expiration) pack) && pack.expiration < Time.fixedTime)
                    queue.Dequeue();
            }

            void IMementoManager.StartRewind()
            {
                Store();

                stack.Clear();

                IEnumerator<(T memento, float expiration)> values = queue.GetEnumerator();
                Debug.Assert(values.MoveNext());
                (T memento, float expiration) last;
                (T memento, float expiration) current = values.Current;
                while (values.MoveNext())
                {
                    last = current;
                    current = values.Current;
                    stack.Push((last.memento, current.expiration - last.expiration));
                }
            }

            void IMementoManager.UpdateRewind(float deltatime)
            {
                float count = 0;
                if (stack.TryPop(out (T memento, float delta) current))
                {
                    count += deltatime;
                    (T memento, float delta) last;
                    while (current.delta < count)
                    {
                        last = current;
                        count -= current.delta;
                        if (!stack.TryPop(out current))
                        {
                            onRewind(last.memento);
                            return;
                        }
                    }

                    if (stack.TryPeek(out (T memento, float delta) next))
                        onRewind(interpolate(current.memento, next.memento, count / current.delta));
                    else
                        onRewind(current.memento);

                    stack.Push(current);
                }
                else
                    onRewind(null);
            }
        }
    }

    public readonly struct StartRewindEvent { }

    public readonly struct StopRewindEvent { }
}