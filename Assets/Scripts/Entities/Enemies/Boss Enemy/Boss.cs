using Asteroids.PowerUps;

using Enderlook.GOAP;

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

        private int currentLifes;

        private const float ClosestDistanceToPlayer = 1;
        private const float FurtherDistanceToPlayer = 15;

        private const int HealthRestoredPerPack = 4;
        private const float AverageTimeRequiredByCloseAttack = .8f;
        private const float AverageTimeRequiredByFarAttack = 1;

        private bool IsTooHurt(BossState state) => (state.BossHealth / (float)lifes) <= tooHurtFactor;

        private Plan<BossState, IAction<BossState, IGoal<BossState>>> plan = new Plan<BossState, IAction<BossState, IGoal<BossState>>>();

        private IAction<BossState, IGoal<BossState>>[] actions;

        private void Awake()
        {
            currentLifes = lifes;

            actions = new IAction<BossState, IGoal<BossState>>[]
            {
                new AttackCloseAction(this),
                new AttackFarAction(this),
                new GetCloserPlayerAction(this),
                new GetFurtherPlayerAction(this),
                new PickPowerUpAction(this),
                new WaitForPowerUpAction(),
            };

            // For gameplay reasons the boss is not tracked by the rewind feature.
            //plan.Plan(new BossState(this), actions).
        }
    }
}