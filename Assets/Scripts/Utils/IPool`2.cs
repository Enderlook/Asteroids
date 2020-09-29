namespace Asteroids.Utils
{
    /// <summary>
    /// A pool of objects that can recycle old objects and create new ones on demand.
    /// </summary>
    /// <typeparam name="TObject">Type of object to create.</typeparam>
    /// <typeparam name="TParameter">Type of parameter used to construct the object.</typeparam>
    public interface IPool<TObject, TParameter> : IFactory<TObject, TParameter>, IRecycler<TObject, TParameter>
    {
    }
}