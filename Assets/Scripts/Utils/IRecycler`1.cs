using System;

namespace Asteroids.Utils
{
    /// <summary>
    /// A recycler to store and reuse instances of objects.
    /// </summary>
    /// <typeparam name="TObject">Type of object to recycle.</typeparam>
    public interface IRecycler<TObject>
    {
        /// <summary>
        /// Extract and initializes an stored instance.
        /// </summary>
        /// <returns>Recycled object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this instance is empty.</exception>
        TObject Get();

        /// <summary>
        /// Stores and deinitialized <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Element to store.</param>
        void Store(TObject obj);

        /// <summary>
        /// Try to extract and initialize an stored instance, if any.
        /// </summary>
        /// <param name="obj">Recycled object, if any.</param>
        /// <returns>Whenever there was an object to recycle or not.</returns>
        bool TryGet(out TObject obj);
    }
}