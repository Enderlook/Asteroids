using Enderlook.Extensions;

using System;
using System.Collections.Generic;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IRecycler{TObject}"/>
    public class Recycler<TObject> : IRecycler<TObject>
    {
        private Stack<TObject> pool;

        private Action<TObject> enable;

        private Action<TObject> disable;

        /// <summary>
        /// Amount of elements stored.
        /// </summary>
        public int Count => pool.Count;

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of stored elements.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance.</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Recycler(int initialCapacity, Action<TObject> enable, Action<TObject> disable)
        {
            pool = new Stack<TObject>(initialCapacity);
            this.enable = enable;
            this.disable = disable;
        }

        /// <inheritdoc cref="Recycler{T}.ctor(int, Action{T1}, Action{T1})"/>
        public Recycler(Action<TObject> enable, Action<TObject> disable)
        {
            pool = new Stack<TObject>();
            this.enable = enable;
            this.disable = disable;
        }

        /// <summary>
        /// Determines if this recycler is empty
        /// </summary>
        /// <returns>Whenever it's empty or not.</returns>
        public bool IsEmpty() => pool.IsEmpty();

        /// <inheritdoc cref="IRecycler{TObject}.Get"/>
        public TObject Get()
        {
            TObject obj = pool.Pop();
            if (!(enable is null))
                enable(obj);
            return obj;
        }

        /// <inheritdoc cref="IRecycler{TObject}.TryGet(out TObject)"/>
        public bool TryGet(out TObject obj)
        {
            if (pool.TryPop(out obj))
            {
                if (!(enable is null))
                    enable(obj);
                return true;
            }
            obj = default;
            return false;
        }

        /// <inheritdoc cref="IRecycler{TObject}.Store(TObject)"/>
        public void Store(TObject obj)
        {
            if (!(disable is null))
                disable(obj);
            pool.Push(obj);
        }
    }
}