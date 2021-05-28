//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;
using Asteroids.Utils;

using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed class ShooterEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

        private readonly Dictionary<Sprite, string> reverseSprites = new Dictionary<Sprite, string>();
        private readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)> builder;
        public ShooterEnemyFlyweight Flyweight {
            get => builder.flyweight;
            set => builder.flyweight = value;
        }

        private string id;

        public ShooterEnemyBuilder(string id)
        {
            builder = new BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>
                {
                    constructor = InnerConstruct,
                    commonInitializer = CommonInitialize,
                    initializer = initialize,
                    deinitializer = deinitialize
                };

            this.id = id;

            GameSaver.SubscribeShooterEnemy(
                (states) =>
                {
                    foreach ((SimpleEnemyBuilder.EnemyState enemyState, Shooter.ShooterState shooterState, List<Shooter.ProjectileState> projectileStates) in states)
                    {
                        GameObject enemy = Create(default);
                        enemyState.Load(this, enemy);
                        enemy.GetComponent<Shooter>().Load(shooterState, projectileStates);
                    }
                }
            );
        }

        private GameObject Construct(in ShooterEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter, IPool<GameObject, (Vector3 position, Vector3 speed)> pool, string id)
        {
            GameObject enemy = SimpleEnemyBuilder.ConstructButNotSave(flyweight, pool, out Rigidbody2D rigidbody, out SpriteRenderer spriteRenderer);

            Shooter shooter = enemy.AddComponent<Shooter>();
            shooter.Construct(flyweight, enemy.transform);

            GameSaver.SubscribeShooterEnemy(shooter, () => new SimpleEnemyBuilder.EnemyState(rigidbody, reverseSprites[spriteRenderer.sprite]));

            return enemy;
        }

        private GameObject InnerConstruct(in ShooterEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
            => Construct(flyweight, parameter, this, id);

        private void CommonInitialize(in ShooterEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
        {
            SimpleEnemyBuilder.CommonInitialize(flyweight, enemy, parameter, reverseSprites);

            Rigidbody2D rigidbody = enemy.GetComponent<Rigidbody2D>();
            Vector2 direction = rigidbody.velocity.normalized;
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemy.transform.rotation = Quaternion.Euler(0, 0, z + 90);
            rigidbody.rotation = z;
        }

        private static void Initialize(in ShooterEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
             => SimpleEnemyBuilder.Initialize(flyweight, enemy, parameter);

        private static void Deinitialize(GameObject enemy)
            => SimpleEnemyBuilder.Deinitialize(enemy);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);
    }
}