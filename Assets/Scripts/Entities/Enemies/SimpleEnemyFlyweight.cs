using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;
using UnityEngine.Audio;
namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemies/Simple Enemy Flyweight")]
    public class SimpleEnemyFlyweight : EnemyFlyweight
    {
#pragma warning disable CS0649
        [field: SerializeField, IsProperty, Tooltip("Points got when destroyed.")]
        public int ScoreWhenDestroyed { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Mass of the enemy")]
        public float Mass { get; private set; }

        [field: SerializeField, Tooltip("A random sprite will be picked by the enemy."), DrawTexture(false), IsProperty]
        public string[] Sprites { get; private set; }

        [field: SerializeField, Layer, IsProperty, Tooltip("Layer of the enemy.")]
        public int Layer { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Sound played on death.")]
        public Sound DeathSound { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Audio mixer group of death sound.")]
        public AudioMixerGroup AudioMixerGroup { get; private set; }
#pragma warning restore CS0649

        protected IFactory<GameObject, (Vector3 position, Vector3 speed)> factory;

        public override IFactory<GameObject, (Vector3 position, Vector3 speed)> GetFactory()
        {
            if (factory is null)
                factory = new SimpleEnemyBuilder { Flyweight = this };
            return factory;
        }
    }
}