using System;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IFactory{TObject, TParameter}"/>
    public class Factory<TObject, TParameter> : IFactory<TObject, TParameter>
    {
        private Func<TParameter, TObject> constructor;

        /// <summary>
        /// Creates a factory of objects.
        /// </summary>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructor"/> is <see langword="null"/>.</exception>
        public Factory(Func<TParameter, TObject> constructor)
        {
            if (constructor is null)
                throw new ArgumentNullException(nameof(constructor));
            this.constructor = constructor;
        }

        /// <inheritdoc cref="IFactory{TObject, TParameter}.Create(TParameter)"/>
        public TObject Create(TParameter parameter) => constructor(parameter);
    }
}