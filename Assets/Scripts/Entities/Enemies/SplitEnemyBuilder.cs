using Asteroids.Events;
using Asteroids.Utils;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public class SplitEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer commonInitialize = CommonInitialize;
        private static readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

        private readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)> builder;
        public SplitEnemyFlyweight Flyweight {
            get => builder.flyweight;
            set => builder.flyweight = value;
        }

        public SplitEnemyBuilder() => builder = new BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>
        {
            constructor = Construct,
            commonInitializer = commonInitialize,
            initializer = initialize,
            deinitializer = deinitialize
        };

        public GameObject Construct(in SplitEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
        {
            GameObject enemy = SimpleEnemyBuilder.Construct(flyweight, parameter, this);

            enemy.AddComponent<SplitOnDeath>().flyweight = flyweight;

            return enemy;
        }

        private static void Initialize(in SplitEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => SimpleEnemyBuilder.Initialize(flyweight, enemy, parameter);

        private static void CommonInitialize(in SplitEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => SimpleEnemyBuilder.CommonInitialize(flyweight, enemy, parameter);

        private static void Deinitialize(GameObject enemy) => SimpleEnemyBuilder.Deinitialize(enemy);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);

        private sealed class SplitOnDeath : ExecuteOnCollision
        {
            public SplitEnemyFlyweight flyweight;

            public override void Execute()
            {
                for (int i = 0; i < flyweight.amountToSpawn; i++)
                    _ = flyweight.enemyToSpawn.GetFactory().Create((transform.position, Random.insideUnitCircle * flyweight.initialSpeed.Value));

                EventManager.Raise(new EnemySplittedEvent(flyweight.amountToSpawn));
            }
        }
    }
}