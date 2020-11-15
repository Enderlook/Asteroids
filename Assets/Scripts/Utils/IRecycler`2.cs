using System;

namespace Asteroids.Utils
{
    /// <summary>
    /// A recycler to store and reuse instances of objects.
    /// </summary>
    /// <typeparam name="TObject">Type of object to recycle.</typeparam>
    /// <typeparam name="TParameter">Type of parameter used to construct the object.</typeparam>
    public interface IRecycler<TObject, TParameter>
    {
        /// <summary>
        /// Extract and initializes an stored instance.
        /// </summary>
        /// <param name="parameter">Configuration of the recycled object.</param>
        /// <returns>Recycled object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this instance is empty.</exception>
        TObject Get(TParameter parameter);

        /// <summary>
        /// Stores and deinitialized <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Element to store.</param>
        void Store(TObject obj);

        /// <summary>
        /// Try to extract and initialize an stored instance, if any.
        /// </summary>
        /// <param name="parameter">Configuration of the recycled object.</param>
        /// <param name="obj">Recycled object, if any.</param>
        /// <returns>Whenever there was an object to recycle or not.</returns>
        bool TryGet(TParameter parameter, out TObject obj);

        /// <summary>
        /// Forces the extraction of an element if it was under the control of this instance.
        /// </summary>
        /// <param name="obj">Element to extract.</param>
        void ExtractIfHas(TObject obj);
    }
}