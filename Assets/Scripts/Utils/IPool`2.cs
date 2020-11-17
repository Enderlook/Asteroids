namespace Asteroids.Utils
{
    /// <summary>
    /// A pool of objects that can recycle old objects and create new ones on demand.
    /// </summary>
    /// <typeparam name="TObject">Type of object to create.</typeparam>
    /// <typeparam name="TParameter">Type of parameter used to construct the object.</typeparam>
    public interface IPool<TObject, TParameter> : IFactory<TObject, TParameter>
    {
        /// <summary>
        /// Stores and deinitialized <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Element to store.</param>
        void Store(TObject obj);

        /// <summary>
        /// Forces the extraction of an element if it was under the control of this instance.
        /// </summary>
        /// <param name="obj">Element to extract.</param>
        void ExtractIfHas(TObject obj);
    }
}