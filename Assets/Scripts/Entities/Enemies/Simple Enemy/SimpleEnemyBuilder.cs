using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Enumerables;
using Enderlook.Unity.Components.ScriptableSound;

using System.Collections.Generic;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    public partial class SimpleEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
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

        private string id;

        public SimpleEnemyBuilder(string id)
        {
            builder = new BuilderFactoryPool<GameObject, SimpleEnemyFlyweight, (Vector3 position, Vector3 speed)>
                {
                    constructor = InnerConstruct,
                    commonInitializer = commonInitialize,
                    initializer = initialize,
                    deinitializer = deinitialize
                };

            this.id = id;

            GameSaver.SubscribeEnemy(
                id,
                (states) =>
                {
                    foreach (EnemyState state in states)
                        state.Load(this, Create(default));
                }
            );
        }

        public static GameObject Construct(in SimpleEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter, IPool<GameObject, (Vector3 position, Vector3 speed)> pool, string id)
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

            Memento.TrackForRewind(pool, rigidbody, spriteRenderer, collider);

            GameSaver.SubscribeEnemy(id, () => new EnemyState(rigidbody, spriteRenderer));

            return enemy;
        }

        private GameObject InnerConstruct(in SimpleEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
            => Construct(flyweight, parameter, this, id);

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

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);
    }
}