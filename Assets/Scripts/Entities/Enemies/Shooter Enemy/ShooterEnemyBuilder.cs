//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;
using Asteroids.Utils;
using Asteroids.WeaponSystem;

using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public class ShooterEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static readonly Dictionary<Sprite, string> sprites = new Dictionary<Sprite, string>();
        private static readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer commonInitialize = CommonInitialize;
        private static readonly BuilderFactoryPool<GameObject, ShooterEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

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
                    commonInitializer = commonInitialize,
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

        public GameObject Construct(in ShooterEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter, IPool<GameObject, (Vector3 position, Vector3 speed)> pool, string id)
        {
            GameObject enemy = SimpleEnemyBuilder.ConstructButNotSave(flyweight, pool, out Rigidbody2D rigidbody, out SpriteRenderer spriteRenderer);

            Shooter shooter = enemy.AddComponent<Shooter>();
            shooter.Construct(flyweight.sprite, flyweight.force, flyweight.ShootSound, flyweight.cooldown, flyweight.projectileLayer, enemy.transform);

            GameSaver.SubscribeShooterEnemy(shooter, () => new SimpleEnemyBuilder.EnemyState(rigidbody, sprites[spriteRenderer.sprite]));

            return enemy;
        }

        private GameObject InnerConstruct(in ShooterEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
            => Construct(flyweight, parameter, this, id);

        public static void CommonInitialize(in ShooterEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => SimpleEnemyBuilder.CommonInitialize(flyweight, enemy, parameter);

        public static void Initialize(in ShooterEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
             => SimpleEnemyBuilder.Initialize(flyweight, enemy, parameter);

        public static void Deinitialize(GameObject enemy)
            => SimpleEnemyBuilder.Deinitialize(enemy);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);
    }
}