using System;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IPool{TObject,}"/>
    public class Pool<TObject> : IPool<TObject>
    {
        private Recycler<TObject> recycler;
        private Factory<TObject> factory;

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of stored elements.</param>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(int initialCapacity, Func<TObject> constructor, Action<TObject> enable, Action<TObject> disable)
        {
            recycler = new Recycler<TObject>(initialCapacity, enable, disable);
            factory = new Factory<TObject>(constructor);
        }

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(Func<TObject> constructor, Action<TObject> enable, Action<TObject> disable)
        {
            recycler = new Recycler<TObject>(enable, disable);
            factory = new Factory<TObject>(constructor);
        }

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.Get(TParameter)"/>
        public TObject Get()
        {
            if (recycler.TryGet(out TObject obj))
                return obj;
            return factory.Create();
        }

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.Store(TObject)"/>
        public void Store(TObject obj) => recycler.Store(obj);

        /// <inheritdoc cref="IFactory{TObject, TParameter}.Create(TParameter)"/>
        TObject IFactory<TObject>.Create() => Get();

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.TryGet(TParameter, out TObject)"/>
        bool IRecycler<TObject>.TryGet(out TObject obj)
        {
            obj = Get();
            return true;
        }
    }
}