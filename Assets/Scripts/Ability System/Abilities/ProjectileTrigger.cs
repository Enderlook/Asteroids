using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Projectile", fileName = "Projectile Ability")]
    public class ProjectileTrigger : Ability
    {
        [SerializeField, Tooltip("The projectile prefab to be instantiated.")]
        private GameObject prefab = null;

        [SerializeField, Tooltip("Damage of the projectile.")]
        private int damage = 0;

        [SerializeField, Tooltip("Speed at which the projectile is fired.")]
        private float speed = 0;

        [SerializeField, Tooltip("Layers to hit.")]
        private LayerMask hitTargets = default;

        private Transform castPoint;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            base.Initialize(abilitiesManager);
            castPoint = abilitiesManager.CastPoint;
        }

        public override void Execute()
        {
            GameObject projectile = Instantiate(prefab, castPoint.position, castPoint.rotation);
            Projectile.AddComponentTo(projectile, damage, speed, hitTargets);
        }
    }
}
