namespace Asteroids.Utils
{
    /// <summary>
    /// A factory of objects.
    /// </summary>
    /// <typeparam name="TObject">Type of object to create.</typeparam>
    public interface IFactory<out TObject>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>New created instance.</returns>
        TObject Create();
    }
}