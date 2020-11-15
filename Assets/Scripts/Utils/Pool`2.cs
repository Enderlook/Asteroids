using System;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IPool{TObject, TParameter}"/>
    public class Pool<TObject, TParameter> : IPool<TObject, TParameter>
    {
        private Recycler<TObject, TParameter> recycler;
        private Factory<TObject, TParameter> factory;

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of stored elements.</param>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(int initialCapacity, Func<TParameter, TObject> constructor, Action<TObject, TParameter> enable, Action<TObject> disable)
        {
            recycler = new Recycler<TObject, TParameter>(initialCapacity, enable, disable);
            factory = new Factory<TObject, TParameter>(constructor);
        }

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(Func<TParameter, TObject> constructor, Action<TObject, TParameter> enable, Action<TObject> disable)
        {
            recycler = new Recycler<TObject, TParameter>(enable, disable);
            factory = new Factory<TObject, TParameter>(constructor);
        }

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.Get(TParameter)"/>
        public TObject Get(TParameter parameter)
        {
            if (recycler.TryGet(parameter, out TObject obj))
                return obj;
            return factory.Create(parameter);
        }

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.Store(TObject)"/>
        public void Store(TObject obj) => recycler.Store(obj);

        /// <inheritdoc cref="IFactory{TObject, TParameter}.Create(TParameter)"/>
        TObject IFactory<TObject, TParameter>.Create(TParameter parameter) => Get(parameter);

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.TryGet(TParameter, out TObject)"/>
        bool IRecycler<TObject, TParameter>.TryGet(TParameter parameter, out TObject obj)
        {
            obj = Get(parameter);
            return true;
        }

        /// <inheritdoc cref="IRecycler{TObject, TParameter}.ExtractIfHas(TObject)"/>
        public void ExtractIfHas(TObject obj) => recycler.ExtractIfHas(obj);
    }
}