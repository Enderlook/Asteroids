using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Raycast", fileName = "Raycast Projectile Ability")]
    public class RaycastTrigger : Ability
    {
        [SerializeField, Tooltip("The projectile prefab to be instantiated.")]
        private GameObject prefab = null;

        [SerializeField, Tooltip("Damage of the raycast.")]
        private int damage = 0;

        [SerializeField, Tooltip("Raycast distance.")]
        private float distance = 0;

        [SerializeField, Tooltip("Layers to hit.")]
        private LayerMask hitTargets = default;

        private Transform castPoint;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            base.Initialize(abilitiesManager);
            castPoint = abilitiesManager.CastPoint;
        }

        public override void Update()
        {
            if (castInput.Execute())
                Execute();
        }

        public override void Execute()
        {
            RaycastHit2D ray = Physics2D.Raycast(castPoint.position, castPoint.up, distance, hitTargets);

            //if (ray)
            //{
            //}

            Debug.DrawRay(castPoint.position, castPoint.up * distance, Color.green);
        }
    }
}
