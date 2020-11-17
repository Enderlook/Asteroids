using Asteroids.Utils;

using UnityEngine;
namespace Asteroids.Entities.Enemies
{
    public abstract class EnemyFlyweight : ScriptableObject
    {
        public abstract void Initialize();

        public abstract IFactory<GameObject, (Vector3 position, Vector3 speed)> GetFactory();
    }
}