using Asteroids.Events;

using Enderlook.Unity.Serializables.Ranges;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemies/Split On Death Enemy")]
    public class EnemyGeneratorSplitOnDeath : EnemyGeneratorSimple<EnemyGeneratorSplitOnDeath, EnemyGeneratorSplitOnDeath.Handler>
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Enemy spawned on death.")]
        private EnemyGenerator enemyToSpawn;

        [SerializeField, Tooltip("Amount of enemies spawned on death.")]
        private int amountToSpawn;

        [SerializeField, Tooltip("Initial speed of spawned enemies.")]
        private RangeFloat initialSpeed;
#pragma warning restore CS0649

        public override void Initialize()
        {
            base.Initialize();
            enemyToSpawn.Initialize();
        }

        protected override Handler Constructor((Vector2 position, Vector2 speed) arguments) => ConstructorBase(this, arguments);

        public new class Handler : EnemyGeneratorSimple<EnemyGeneratorSplitOnDeath, Handler>.Handler
        {
            public override void ReturnToPool() => StoreInPool(this);

            protected override void ExecuteOnCollision()
            {
                base.ExecuteOnCollision();

                for (int i = 0; i < Data.amountToSpawn; i++)
                    _ = Data.enemyToSpawn.Create((Rigidbody.position, Random.insideUnitCircle * Data.initialSpeed.Value));
            }

            protected override EnemyDestroyedEvent CreateEnemyDestroyedEvent()
                => base.CreateEnemyDestroyedEvent().WithNewSpawnedEnemiesCount(Data.amountToSpawn);
        }
    }
}