//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemies/Bomber Enemy Flyweight")]
    public sealed class BomberEnemyFlyweight : SimpleEnemyFlyweight
    {
#pragma warning disable CS0649
        [field: SerializeField, IsProperty, Tooltip("Sound played on shoot.")]
        public Sound ShootSound { get; private set; }

        [field: SerializeField, DrawTexture(true), IsProperty, Tooltip("Sprite of the bomb to fire.")]
        public string Sprite { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Sprite scale of the projectile to fire.")]
        public float SpriteScale { get; private set; }

        [field: SerializeField, IsProperty, Min(0), Tooltip("Force at which the projectile is fired.")]
        public float Force { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Time between attacks.")]
        public float Cooldown { get; private set; }

        [field: SerializeField, Layer, IsProperty, Tooltip("Layer of the projectile.")]
        public int BombLayer { get; private set; }

        [field: SerializeField, IsProperty, Min(0), Tooltip("Distance of player at bomb to explode.")]
        public float ExplosionDistance { get; private set; }

        [field: SerializeField, Tooltip("Animation used to explote the bomb.")]
        public RuntimeAnimatorController ExplodeAnimation { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Sprite scale of the explosion.")]
        public float ExplodeScale { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Sound played on explode.")]
        public AudioClip ExplodeSound { get; private set; }

        [field: SerializeField, Layer, IsProperty, Tooltip("Layer of the explosion.")]
        public int ExplodeLayer { get; private set; }
#pragma warning restore CS0649

        public override void Initialize() => factory = new BomberEnemyBuilder(name) { Flyweight = this };
    }
}