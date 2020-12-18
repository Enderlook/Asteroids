using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IPool{TObject, TParameter}"/>
    public class Pool<TObject, TParameter> : IPool<TObject, TParameter>
    {
        private const BindingFlags BindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FieldInfo _array = typeof(Stack<TObject>).GetField("_array", BindingAttr);
        private static readonly FieldInfo _size = typeof(Stack<TObject>).GetField("_size", BindingAttr);
        private static readonly FieldInfo _version = typeof(Stack<TObject>).GetField("_version", BindingAttr);

        private Func<TParameter, TObject> constructor;
        private Stack<TObject> pool;
        private Action<TObject, TParameter> enable;
        private Action<TObject> disable;

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of stored elements.</param>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(int initialCapacity, Func<TParameter, TObject> constructor, Action<TObject, TParameter> enable, Action<TObject> disable)
        {
            this.constructor = constructor;
            this.enable = enable;
            this.disable = disable;
            pool = new Stack<TObject>(initialCapacity);
        }

        /// <summary>
        /// Creates a recycler of objects.
        /// </summary>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <param name="enable">Action executed to initialize an object when retrived from this instance. This is not executed new instances, only recycled ones..</param>
        /// <param name="disable">Action executed to deinitialize an object when stored in this instance.</param>
        public Pool(Func<TParameter, TObject> constructor, Action<TObject, TParameter> enable, Action<TObject> disable)
        {
            this.constructor = constructor;
            this.enable = enable;
            this.disable = disable;
            pool = new Stack<TObject>();
        }

        /// <inheritdoc cref="IFactory{TObject, TParameter}.Create(TParameter)"/>
        public TObject Create(TParameter parameter)
        {
            if (pool.TryPop(out TObject obj))
            {
                if (!(enable is null))
                    enable(obj, parameter);
                return obj;
            }
            
            return constructor(parameter);
        }

        /// <inheritdoc cref="IPool{TObject, TParameter}.Store(TObject)"/>
        public void Store(TObject obj)
        {
#if UNITY_EDITOR
            if (pool.Contains(obj))
                Debug.LogError("Storing twice same object in pool.");
#endif

            if (!(disable is null))
                disable(obj);
            pool.Push(obj);
        }

        /// <inheritdoc cref="IPool{TObject, TParameter}.ExtractIfHas(TObject)"/>
        public void ExtractIfHas(TObject obj)
        {
            if (pool.Contains(obj))
            {
                TObject[] array = (TObject[])_array.GetValue(pool);
                int index = Array.IndexOf(array, obj);
                int size = (int)_size.GetValue(pool) - 1;
                if (index < size)
                    Array.Copy(array, index + 1, array, index, size - index);
                array[size] = default;
                _size.SetValue(pool, size);
                _version.SetValue(pool, (int)_version.GetValue(pool) + 1);
            }
        }

        public bool IsInPool(TObject obj) => pool.Contains(obj);
    }
}