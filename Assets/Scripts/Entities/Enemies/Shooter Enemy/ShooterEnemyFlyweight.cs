//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemies/Shooter Enemy Flyweight")]
    public sealed class ShooterEnemyFlyweight : SimpleEnemyFlyweight
    {
#pragma warning disable CS0649
        [field: SerializeField, IsProperty, Tooltip("Sound played on shoot.")]
        public Sound ShootSound { get; private set; }

        [field: SerializeField, DrawTexture(true), IsProperty, Tooltip("Sprite of the projectile to fire.")]
        public string Sprite { get; private set; }

        [field: SerializeField, IsProperty, Min(0), Tooltip("Force at which the projectile is fired.")]
        public float Force { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Time between attacks.")]
        public float Cooldown { get; private set; }

        [field: SerializeField, Layer, IsProperty, Tooltip("Layer of the projectile.")]
        public int ProjectileLayer { get; private set; }
#pragma warning restore CS0649

        public override void Initialize() => factory = new ShooterEnemyBuilder(name) { Flyweight = this };
    }
}