using Asteroids.PowerUps;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss : MonoBehaviour
    {
        [SerializeField, Tooltip("Maximum life.")]
        private int lifes;

        [SerializeField, Tooltip("Movement speed.")]
        private float movementSpeed;

        [SerializeField, Tooltip("Rotation speed.")]
        private float rotationSpeed;

        [SerializeField, Range(0, 1), Tooltip("Life factor considered too hurt.")]
        private float tooHurtFactor;

        private float currentLifes;

        private PowerUpManager powerUpManager;

        private const float ClosestDistanceToPlayer = 1;
        private const float FurtherDistanceToPlayer = 15;

        private const int HealthRestoredPerPack = 4;
        private const float AverageTimeRequiredByCloseAttack = .8f;
        private const float AverageTimeRequiredByFarAttack = 1;

        private void Awake()
        {
            currentLifes = lifes;
            powerUpManager = FindObjectOfType<PowerUpManager>();

            // For gameplay reasons the boss is not tracked by the rewind feature.
        }
    }
}