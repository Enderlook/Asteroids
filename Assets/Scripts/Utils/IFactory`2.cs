namespace Asteroids.Utils
{
    /// <summary>
    /// A factory of objects.
    /// </summary>
    /// <typeparam name="TObject">Type of object to create.</typeparam>
    /// <typeparam name="TParameter">Type of parameter used to construct the object.</typeparam>
    public interface IFactory<out TObject, in TParameter>
    {
        /// <summary>
        /// Creates a new instance using the parameters from <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">Configuration of the new instance.</param>
        /// <returns>New created instance.</returns>
        TObject Create(TParameter parameter);
    }
}