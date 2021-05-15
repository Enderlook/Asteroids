using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Scene
{
    public sealed partial class GlobalMementoManager
    {
        public sealed class MementoManager<T> : IMementoManager where T : struct
        {
            // We store Memento objects in an stongly typed fashion to avoid boxing and so reduce GC pressure

            private readonly Queue<(T memento, float expiration)> queue = new Queue<(T memento, float expiration)>(aproximateStoredmementos);

            // Profiling shows that this optimization reduces allocation by a factor of x4 when stack is smaller than queue
            // and execution time by x2 when calling StartRewind()
            private readonly Stack<(T memento, float delta)> stack = new Stack<(T memento, float delta)>(aproximateStoredmementos);
            private Func<T> onStore;
            private Func<T, T, float, T> interpolate;
            private Action<T?> onRewind;

            private float count = 0;
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
                if (stack.TryPop(out (T memento, float delta) current))
                {
                    count += deltatime;
                    (T memento, float delta) last;
                    float lastCount = count;
                    while (current.delta < count)
                    {
                        last = current;
                        lastCount = count;
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
}