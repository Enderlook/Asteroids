using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Raycast", fileName = "Raycast Projectile Ability")]
    public class LaserTrigger : Ability
    {
        [SerializeField, Tooltip("Raycast distance.")]
        private float distance = 0;

        [SerializeField, Tooltip("Layers to hit.")]
        private LayerMask hitTargets = default;

        private Transform castPoint;
        private LineRenderer lineRenderer;

        private SimpleSoundPlayer soundPlayer;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            base.Initialize(abilitiesManager);
            castPoint = abilitiesManager.CastPoint;
            lineRenderer = castPoint.GetComponent<LineRenderer>();
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(abilitySound, false, false);
        }

        public override void Update()
        {
            if (castInput.Execute())
                Execute();
            else
                lineRenderer.enabled = false;
        }

        public override void Execute()
        {
            lineRenderer.enabled = true;
            RaycastHit2D ray = Physics2D.Raycast(castPoint.position, castPoint.up, distance, hitTargets);
            if (ray)
            {
                Transform hit = ray.collider.gameObject.transform;
            }
            else
                lineRenderer.SetPosition(0, new Vector3(0, distance, 0));

            soundPlayer.Play();
        }
    }
}
