using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class SplitEnemyBuilder
    {
        private sealed class SplitOnDeath : ExecuteOnCollision
        {
            public SplitEnemyFlyweight flyweight;

            public override void Execute()
            {
                for (int i = 0; i < flyweight.AmountToSpawn; i++)
                    _ = flyweight.EnemyToSpawn.GetFactory().Create((transform.position, Random.insideUnitCircle * flyweight.InitialSpeed.Value));

                EventManager.Raise(new EnemySplittedEvent(flyweight.AmountToSpawn));
            }
        }
    }
}