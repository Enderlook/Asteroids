//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;
using Asteroids.Utils;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed class BomberEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static readonly BuilderFactoryPool<GameObject, BomberEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, BomberEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

        private readonly Dictionary<Sprite, string> reverseSprites = new Dictionary<Sprite, string>();
        private readonly BuilderFactoryPool<GameObject, BomberEnemyFlyweight, (Vector3 position, Vector3 speed)> builder;
        public BomberEnemyFlyweight Flyweight {
            get => builder.flyweight;
            set => builder.flyweight = value;
        }

        private string id;

        public BomberEnemyBuilder(string id)
        {
            builder = new BuilderFactoryPool<GameObject, BomberEnemyFlyweight, (Vector3 position, Vector3 speed)>
                {
                    constructor = InnerConstruct,
                    commonInitializer = CommonInitialize,
                    initializer = initialize,
                    deinitializer = deinitialize
                };

            this.id = id;

            GameSaver.SubscribeBomberEnemy(
                (states) =>
                {
                    foreach ((SimpleEnemyBuilder.EnemyState enemyState, Bomber.BomberState shooterState, List<Bomber.ProjectileState> projectileStates) in states)
                    {
                        GameObject enemy = Create(default);

                        GameManager.StartCoroutine(Work());

                        IEnumerator Work()
                        {
                            yield return null;
                            enemyState.Load(this, enemy);
                            enemy.GetComponent<Bomber>().Load(shooterState, projectileStates);
                        }
                    }
                }
            );
        }

        public GameObject Construct(in BomberEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter, IPool<GameObject, (Vector3 position, Vector3 speed)> pool, string id)
        {
            GameObject enemy = SimpleEnemyBuilder.ConstructButNotSave(flyweight, pool, out Rigidbody2D rigidbody, out SpriteRenderer spriteRenderer);

            Bomber shooter = enemy.AddComponent<Bomber>();
            shooter.Construct(flyweight, enemy.transform);

            GameSaver.SubscribeBomberEnemy(shooter, () => new SimpleEnemyBuilder.EnemyState(rigidbody, reverseSprites[spriteRenderer.sprite]));

            return enemy;
        }

        private GameObject InnerConstruct(in BomberEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
            => Construct(flyweight, parameter, this, id);

        public void CommonInitialize(in BomberEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
        {
            SimpleEnemyBuilder.CommonInitialize(flyweight, enemy, parameter, reverseSprites);

            Rigidbody2D rigidbody = enemy.GetComponent<Rigidbody2D>();
            Vector2 direction = rigidbody.velocity.normalized;
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemy.transform.rotation = Quaternion.Euler(0, 0, z + 90);
            rigidbody.rotation = z;
        }

        public static void Initialize(in BomberEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
             => SimpleEnemyBuilder.Initialize(flyweight, enemy, parameter);

        public static void Deinitialize(GameObject enemy)
            => SimpleEnemyBuilder.Deinitialize(enemy);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);
    }
}