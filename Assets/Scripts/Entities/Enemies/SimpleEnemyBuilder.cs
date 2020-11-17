using Asteroids.Events;
using Asteroids.Utils;

using Enderlook.Enumerables;
using Enderlook.Unity.Components.ScriptableSound;

using System;
using System.Collections.Generic;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    public class SimpleEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static List<Vector2> physicsShape = new List<Vector2>();
        private static readonly BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer commonInitialize = CommonInitialize;
        private static readonly BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

        private readonly BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)> builder;
        public SimpleEnemyFlyweight Flyweight {
            get => builder.flyweight;
            set => builder.flyweight = value;
        }

        public SimpleEnemyBuilder() => builder = new BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)>
        {
            constructor = InnerConstruct,
            commonInitializer = commonInitialize,
            initializer = initialize,
            deinitializer = deinitialize
        };

        public static GameObject Construct(in SimpleEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter, IPool<GameObject, (Vector3 position, Vector3 speed)> pool)
        {
            GameObject enemy = new GameObject(flyweight.name)
            {
                layer = flyweight.Layer,
            };

            Rigidbody2D rigidbody = enemy.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.mass = flyweight.Mass;

            SpriteRenderer spriteRenderer = enemy.AddComponent<SpriteRenderer>();
            PolygonCollider2D collider = enemy.AddComponent<PolygonCollider2D>();
            enemy.AddComponent<ScreenWrapper>();

            SimpleSoundPlayer player = SimpleSoundPlayer.CreateOneTimePlayer(flyweight.DeathSound, false, false);
            player.GetComponent<AudioSource>().outputAudioMixerGroup = flyweight.AudioMixerGroup;

            ExecuteOnDeath executeOnDeath = enemy.AddComponent<ExecuteOnDeath>();
            executeOnDeath.flyweight = flyweight;
            executeOnDeath.pool = pool;
            executeOnDeath.player = player;

            GlobalMementoManager.Subscribe(CreateMemento, ConsumeMemento, interpolateMementos);

            return enemy;

            (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite) CreateMemento()
            {
                bool enabled = rigidbody.gameObject.activeSelf; // Read from rigidbody to reduce closure size
                Vector3 position = rigidbody.position;
                float rotation = rigidbody.rotation;
                Vector2 velocity = rigidbody.velocity;
                float angularVelocity = rigidbody.angularVelocity;
                Sprite sprite = spriteRenderer.sprite;

                // The memento object is simple, so we store it as a tuple
                return (enabled, position, rotation, velocity, angularVelocity, sprite);
            }

            void ConsumeMemento((bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite)? memento)
            {
                if (memento.HasValue)
                {
                    (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite) memento_ = memento.Value;
                    if (memento_.enabled)
                    {
                        // Since enemies are pooled, we must force the pool to give us control of this instance in case it was in his control.
                        pool.ExtractIfHas(rigidbody.gameObject); // Read from rigidbody to reduce closure size

                        rigidbody.position = memento_.position;
                        rigidbody.rotation = memento_.rotation;
                        rigidbody.velocity = memento_.velocity;
                        rigidbody.angularVelocity = memento_.angularVelocity;
                        spriteRenderer.sprite = memento_.sprite;

                        int count = memento_.sprite.GetPhysicsShapeCount();
                        for (int i = 0; i < count; i++)
                        {
                            memento_.sprite.GetPhysicsShape(i, physicsShape);
                            collider.SetPath(i, physicsShape);
                        }
                    }
                    else
                        pool.Store(rigidbody.gameObject); // Read from rigidbody to reduce closure size
                }
                else
                    pool.Store(rigidbody.gameObject); // Read from rigidbody to reduce closure size
            }
        }

        private readonly static Func<(bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite), (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite), float, (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite)> interpolateMementos = InterpolateMementos;

        private static (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite) InterpolateMementos(
            (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite) a,
            (bool enabled, Vector3 position, float rotation, Vector2 velocity, float angularVelocity, Sprite sprite) b,
            float delta
            ) => (
            delta > .5f ? b.enabled : a.enabled,
            Vector3.Lerp(a.position, b.position, delta),
            Mathf.LerpAngle(a.rotation, b.rotation, delta),
            Vector2.Lerp(a.velocity, b.velocity, delta),
            Mathf.Lerp(a.angularVelocity, b.angularVelocity, delta),
            delta > .5f ? b.sprite : a.sprite
            );

        private GameObject InnerConstruct(in SimpleEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
            => Construct(flyweight, parameter, this);

        public static void CommonInitialize(in SimpleEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
        {
            // Don't use Rigidbody to set position because it has one frame delay
            enemy.transform.position = parameter.position;

            Rigidbody2D rigidbody = enemy.GetComponent<Rigidbody2D>();
            rigidbody.velocity = parameter.speed;
            rigidbody.rotation = 0;

            Sprite sprite = Resources.Load<Sprite>(flyweight.Sprites.RandomPick());

            enemy.GetComponent<SpriteRenderer>().sprite = sprite;

            PolygonCollider2D collider = enemy.GetComponent<PolygonCollider2D>();
            int count = sprite.GetPhysicsShapeCount();
            for (int i = 0; i < count; i++)
            {
                sprite.GetPhysicsShape(i, physicsShape);
                collider.SetPath(i, physicsShape);
            }
        }

        public static void Initialize(in SimpleEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => enemy.SetActive(true);

        public static void Deinitialize(GameObject enemy) => enemy.SetActive(false);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public GameObject Get((Vector3 position, Vector3 speed) parameter) => builder.Get(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public bool TryGet((Vector3 position, Vector3 speed) parameter, out GameObject obj) => builder.TryGet(parameter, out obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);

        public sealed class ExecuteOnDeath : ExecuteOnCollision
        {
            public SimpleEnemyFlyweight flyweight;
            public IPool<GameObject, (Vector3 position, Vector3 speed)> pool;
            public SimpleSoundPlayer player;

            public override void Execute()
            {
                player.Play();
                EventManager.Raise(new EnemyDestroyedEvent(flyweight.ScoreWhenDestroyed));
                pool.Store(gameObject);
            }
        }
    }
}