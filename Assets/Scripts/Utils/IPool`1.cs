namespace Asteroids.Utils
{
    /// <summary>
    /// A pool of objects that can recycle old objects and create new ones on demand.
    /// </summary>
    /// <typeparam name="TObject">Type of object to create.</typeparam>
    public interface IPool<TObject> : IFactory<TObject>, IRecycler<TObject>
    {
    }
}