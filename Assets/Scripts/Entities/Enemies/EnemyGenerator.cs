using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public abstract class EnemyGenerator : ScriptableObject, IFactory<IEnemyHandler, (Vector2 position, Vector2 speed)>
    {
        public abstract IEnemyHandler Create((Vector2 position, Vector2 speed) parameter);

        public abstract void Initialize();
    }
}