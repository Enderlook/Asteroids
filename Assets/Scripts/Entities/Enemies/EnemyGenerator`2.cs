using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public abstract class EnemyGenerator<TData, THandler> : EnemyGenerator
        where TData : EnemyGenerator<TData, THandler>
        where THandler : EnemyGenerator<TData, THandler>.Handler, new()
    {
        private Pool<THandler, (Vector2 position, Vector2 speed)> pool;

        public override void Initialize() => pool = new Pool<THandler, (Vector2 position, Vector2 speed)>(Constructor, Initializer, Deinitializer);

        protected abstract THandler Constructor((Vector2 position, Vector2 speed) arguments);

        protected THandler ConstructorBase(TData data, (Vector2 position, Vector2 speed) arguments)
        {
            THandler handler = new THandler();
            handler.Create(data);
            handler.ConfigureRigidbody(arguments.position, arguments.speed);
            return handler;
        }

        private void Initializer(THandler handler, (Vector2 position, Vector2 speed) arguments)
        {
            handler.Initialize();
            handler.ConfigureRigidbody(arguments.position, arguments.speed);
        }

        private void Deinitializer(THandler handler) => handler.Deinitialize();

        public override IEnemyHandler Create((Vector2 position, Vector2 speed) arguments) => pool.Get(arguments);

        public abstract class Handler : IEnemyHandler
        {
            protected TData Data { get; private set; }
            protected GameObject GameObject { get; private set; }
            protected Rigidbody2D Rigidbody { get; private set; }

            public virtual void Create(TData data)
            {
                Data = data;
                GameObject = new GameObject(data.name);
                Rigidbody = GameObject.AddComponent<Rigidbody2D>();
                Rigidbody.gravityScale = 0;
            }

            public virtual void Initialize() => GameObject.SetActive(true);

            public void Deinitialize() => GameObject.SetActive(false);

            public void ConfigureRigidbody(Vector2 position, Vector2 speed)
            {
                // Don't use Rigidbody to set position because it has one frame delay
                GameObject.transform.position = position;

                Rigidbody.velocity = speed;
                Rigidbody.rotation = 0;
            }

            protected void StoreInPool(THandler self) => Data.pool.Store(self);

            public abstract void ReturnToPool();
        }
    }
}