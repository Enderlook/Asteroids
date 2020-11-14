using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;
using UnityEngine.Audio;
namespace Asteroids.Entities.Enemies
{
    public abstract class EnemyFlyweight : ScriptableObject
    {
        public abstract IFactory<GameObject, (Vector3 position, Vector3 speed)> GetFactory();
    }
}