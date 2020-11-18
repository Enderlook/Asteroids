using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class SimpleEnemyBuilder
    {
        public sealed class ExecuteOnDeath : ExecuteOnCollision
        {
            public SimpleEnemyFlyweight flyweight;
            public IPool<GameObject, (Vector3 position, Vector3 speed)> pool;
            public SimpleSoundPlayer player;

            public override void Execute()
            {
                player.Play();
                EventManager.Raise(new EnemySpawner.EnemyDestroyedEvent(flyweight.ScoreWhenDestroyed));
                pool.Store(gameObject);
            }
        }
    }
}