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
                if (player != null) // The check prevent an odd bug when loading a level. TODO: improve this.
                    player.Play();
                //IA2-P3
                // ^- Don't touch that comment, used by the teacher
                EventManager.Raise(new EnemySpawner.EnemyDestroyedEvent(flyweight.name, flyweight.ScoreWhenDestroyed));
                pool.Store(gameObject);
            }
        }
    }
}