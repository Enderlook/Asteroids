﻿using Asteroids.Entities.Enemies;
using Asteroids.Scene;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Raycast", fileName = "Raycast Projectile Ability")]
    public partial class LaserTrigger : Ability
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Raycast distance.")]
        private float distance;

        [SerializeField, Tooltip("Layers to hit.")]
        private LayerMask hitTargets;

        [SerializeField, Tooltip("Laser duration in seconds.")]
        private float duration;
#pragma warning restore CS0649

        private Transform castPoint;
        private LineRenderer lineRenderer;

        private SimpleSoundPlayer soundPlayer;

        private float currentDuration;
        private RaycastHit2D[] results;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            base.Initialize(abilitiesManager);
            castPoint = abilitiesManager.CastPoint;
            lineRenderer = castPoint.GetComponent<LineRenderer>();
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(abilitySound, false, false);
            results = new RaycastHit2D[FindObjectOfType<EnemySpawner>().MaxmiumAmountOfEnemies];

            Memento.TrackForRewind(this);
        }

        public override void Update()
        {
            base.Update();

            if (GlobalMementoManager.IsRewinding)
                return;

            if (currentDuration > 0)
            {
                currentDuration -= Time.deltaTime;
                if (currentDuration <= 0)
                {
                    lineRenderer.enabled = false;
                    return;
                }
                UpdateLaser();
            }
        }

        private void UpdateLaser()
        {
            int amount = Physics2D.RaycastNonAlloc(castPoint.position, castPoint.up, results, 100, hitTargets);
            for (int i = 0; i < amount; i++)
            {
                GameObject gameObject = results[i].collider.gameObject;
                foreach (ExecuteOnCollision toExecute in gameObject.GetComponents<ExecuteOnCollision>())
                    toExecute.Execute();
            }
            lineRenderer.SetPosition(0, new Vector3(0, distance, 0));
        }

        public override void Execute()
        {
            currentDuration = duration;
            lineRenderer.enabled = true;
            soundPlayer.Play();
            UpdateLaser();
        }
    }
}