using System;

namespace Asteroids.Utils
{
    /// <inheritdoc cref="IFactory{TObject,}"/>
    public class Factory<TObject> : IFactory<TObject>
    {
        private Func<TObject> constructor;

        /// <summary>
        /// Creates a factory of objects.
        /// </summary>
        /// <param name="constructor">Constructor of the objects.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructor"/> is <see langword="null"/>.</exception>
        public Factory(Func<TObject> constructor)
        {
            if (constructor is null)
                throw new ArgumentNullException(nameof(constructor));
            this.constructor = constructor;
        }

        /// <inheritdoc cref="IFactory{TObject}.Create()"/>
        public TObject Create() => constructor();
    }
}