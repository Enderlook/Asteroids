namespace Asteroids.Utils
{
    public sealed class BuilderFactoryPool<TObject, TFlyweight, TParameter> : IPool<TObject, TParameter>
    {
        public delegate TObject Constructor(in TFlyweight flyweight, in TParameter parameter);
        public delegate void Initializer(in TFlyweight flyweight, TObject obj, in TParameter parameter);
        public delegate void Deinitializer(TObject obj);

        private readonly Pool<TObject, TParameter> pool;

        public TFlyweight flyweight;
        public Constructor constructor;
        public Initializer initializer;
        public Initializer commonInitializer;
        public Deinitializer deinitializer;

        public BuilderFactoryPool()
        {
            pool = new Pool<TObject, TParameter>(ConstructFirstTime, Initialize, Deinitialize);
        }

        private TObject ConstructFirstTime(TParameter parameter)
        {
            TObject obj = constructor(flyweight, parameter);
            commonInitializer(flyweight, obj, parameter);
            return obj;
        }

        private void Initialize(TObject obj, TParameter parameter)
        {
            commonInitializer(flyweight, obj, parameter);
            initializer(flyweight, obj, parameter);
        }

        private void Deinitialize(TObject obj) => deinitializer(obj);

        public TObject Create(TParameter parameter) => pool.Create(parameter);

        public void Store(TObject obj) => pool.Store(obj);

        public void ExtractIfHas(TObject obj) => pool.ExtractIfHas(obj);
    }
}