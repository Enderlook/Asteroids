using System;

//MyA1-P1

/* This class Adapts Pool.
 * Pool class only works on items which implements IPooleable according to the exercise
 * However that interface doesn't exists. Instead it exist IPoolObject, so we adapt according to that.
 * 
 * Normally we would add the IPoolObject contraint in the generict parameter T of PoolAdapter
 * in order to easelly adapt both types,
 * however the exercise doesn't tell that T of PoolAdapter<T> has such contraint,
 * and so we are forced to also adapt the pooled elements so T can be unconstrained.
 */

public sealed class PoolAdapter<T>
{
    private readonly Pool pool;

    public PoolAdapter(Func<T> createFunction, Action<T> disableFunction, int initialQuantity) => pool = new Pool(
            () => new PooledObjectAdapter(createFunction()),
            (e) => disableFunction(((PooledObjectAdapter)e).element),
            initialQuantity
        );

    /* According to the exercise, GetObject has the firm `T GetObject<T>()`,
     * which suggest that this `T` is defined in the method and not in the class.
     * However that would produce a naming scope error and also doesn't make sense.
     * So we ignore that and use the `T` defined by the type.
     */
    public T GetObject() => ((PooledObjectAdapter)pool.AcquireObject()).element;

    public void ReturnObject(object obj)
    {
        // We don't use `is` operator because it would also allow assignable types, and we want exact matches 
        if (obj.GetType() != typeof(T))
            throw new ArgumentException($"Type of {nameof(obj)} must be the same of {nameof(T)} generic parameter. Was {obj.GetType()}", nameof(obj));

        pool.ReleaseObject(new PooledObjectAdapter((T)obj));
    }

    private sealed class PooledObjectAdapter : IPoolObject
    {
        public readonly T element;

        public PooledObjectAdapter(T element) => this.element = element;

        public void OnAcquire() { }

        public void OnDispose() { }
    }
}

// However, we also provide a variant of the adapter which may be the intended exercise.
public sealed class PoolAdapter2<T> where T : IPoolObject
{
    private readonly Pool pool;

    public PoolAdapter2(Func<T> createFunction, Action<T> disableFunction, int initialQuantity) => pool = new Pool(
            () => createFunction(),
            (e) => disableFunction((T)e),
            initialQuantity
        );

    public T GetObject() => (T)pool.AcquireObject();

    public void ReturnObject(object obj)
    {
        // We don't use `is` operator because it would also allow assignable types, and we want exact matches 
        if (obj.GetType() != typeof(T))
            throw new ArgumentException($"Type of {nameof(obj)} must be the same of {nameof(T)} generic parameter. Was {obj.GetType()}", nameof(obj));

        pool.ReleaseObject((T)obj);
    }
}