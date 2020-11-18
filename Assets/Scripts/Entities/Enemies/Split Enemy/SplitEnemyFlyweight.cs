using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Serializables.Ranges;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemies/Split Enemy Flyweight")]
    public class SplitEnemyFlyweight : SimpleEnemyFlyweight
    {
#pragma warning disable CS0649
        [field: SerializeField, IsProperty, Tooltip("Enemy spawned on death.")]
        public SimpleEnemyFlyweight enemyToSpawn { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Amount of enemies spawned on death.")]
        public int amountToSpawn { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Initial speed of spawned enemies.")]
        public RangeFloat initialSpeed { get; private set; }
#pragma warning restore CS0649

        public override IFactory<GameObject, (Vector3 position, Vector3 speed)> GetFactory()
        {
            if (factory is null)
                factory = new SplitEnemyBuilder(name) { Flyweight = this};
            return factory;
        }
    }
}