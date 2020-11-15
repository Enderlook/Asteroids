using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_GlobalMementoManager)]
    public class GlobalMementoManager : MonoBehaviour
    {
        private static GlobalMementoManager instance;

        private const float expirationTime = 5;

        /*
         * It may look crazy to store 30 mementos per seconds per object, but actually it's not so crazy:
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
         * A bullet memento is (bool, Vector3, float, Vector2, float). That is 29 bytes.         * 
         * Bullets are shooted each 0.35 seconds with a force of 10 and mass of 1 and the boundary is at 10 offset,
         * and largest distances are diagonal.
         * So there can be (1 / 0.35) * (((10^2 + 10^2) ^ 0.5) / (10 / 1)) = 4,04 = 5 instance on memory.
         * 
         * The total size of mementos per collection is 84 * 48 + 28 + 4 + 4 + 4 + 4 * 29 = 4.188 bytes = 4.08 kb.
         * 
         * The total size of mementos in memory is 4.188 * 30 * 5 = 628.200 bytes = 613.47 kb, not an huge deal.
         * 
         * In terms of allocations, 4.188 * 30 = 125.640 bytes/s = 122.69 kb/s is an huge deal.
         * However, our mementos are value types, so we are not allocating/deallocating 122.366 kb/s.
         * Instead value types are inlined in the underlying array of the queue (which is implemented as a circular array),
         * so actually allocations only happens during the first 5 seconds until queues reaches their maximum size and no longer resize.
         */
        private const float storeCooldown = 1f / 30;

        private List<IMementoManager> managers = new List<IMementoManager>();
        private float cooldown;

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
        private void Update()
        {
            cooldown -= Time.deltaTime;
            if (cooldown < 0)
            {
                cooldown = storeCooldown;
                foreach (IMementoManager manager in managers)
                    manager.Store();
            }
        }

        public static void Rewind()
        {
            foreach (IMementoManager manager in instance.managers)
                manager.Rewind();
        }

        public static void Subscribe<T>(Func<T> onStore, Action<T> onRewind)
        {
            // We don't require to remove callbacks never because we this object and its originators are stop being used at the same time

            MementoManager<T> manager = new MementoManager<T>(onStore, onRewind);
            instance.managers.Add(manager);
        }

        private interface IMementoManager
        {
            void Store();
            void Rewind();
        }

        public class MementoManager<T> : IMementoManager
        {
            // We store Memento objects in an stongly typed fashion to avoid boxing and so reduce GC pressure

            private readonly Queue<(T memento, float expiration)> states = new Queue<(T memento, float expiration)>();
            private Func<T> onStore;
            private Action<T> onRewind;

            public MementoManager(Func<T> onStore, Action<T> onRewind)
            {
                this.onRewind = onRewind;
                this.onStore = onStore;
            }

            void IMementoManager.Store() => Store();

            private void Store()
            {
                T memento = onStore();
                states.Enqueue((memento, Time.time + expirationTime));
                while (states.TryPeek(out (T _, float expiration) pack) && pack.expiration < Time.time)
                    _ = states.Dequeue();
            }

            void IMementoManager.Rewind()
            {
                // Prevent error when memento is stored.
                Store();
                onRewind(states.Dequeue().memento);
            }
        }
    }
}