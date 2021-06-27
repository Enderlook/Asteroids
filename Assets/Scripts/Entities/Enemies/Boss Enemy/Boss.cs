using Enderlook.GOAP;

using System.Text;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [DefaultExecutionOrder(10)]
    public sealed partial class Boss : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Maximum life.")]
        private int lifes;

        [SerializeField, Tooltip("Movement speed.")]
        private float movementSpeed;

        [SerializeField, Tooltip("Rotation speed.")]
        private float rotationSpeed;

        [SerializeField, Range(0, 1), Tooltip("Life factor considered too hurt.")]
        private float tooHurtFactor;
#pragma warning restore CS0649

        private int currentLifes;

        private const float ClosestDistanceToPlayer = 1;
        private const float FurtherDistanceToPlayer = 15;

        private const int HealthRestoredPerPack = 4;
        private const float AverageTimeRequiredByCloseAttack = .8f;
        private const float AverageTimeRequiredByFarAttack = 1;

        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> currentPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> inProgressPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private int currentStep;

        private readonly IAction<BossState, IGoal<BossState>>[] actions = new IAction<BossState, IGoal<BossState>>[6];
        private readonly PlayerIsDeadGoal goal = new PlayerIsDeadGoal();

        private PlanningCoroutine<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> planification;
        private float nextPlanificationAt;
        private const float timeBetweenPlanifications = 2f;

#if UNITY_EDITOR
        private StringBuilder builder = new StringBuilder();
        private const bool log = true;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            currentLifes = lifes;

            actions[0] = new AttackCloseAction(this);
            actions[1] = new AttackFarAction(this);
            actions[2] = new GetCloserPlayerAction(this);
            actions[3] = new GetFurtherPlayerAction(this);
            actions[4] = new PickPowerUpAction(this);
            actions[5] = new WaitForPowerUpAction();

            currentStep = -1;
            CheckPlanification();

            // For gameplay reasons the boss is not tracked by the rewind feature.
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            CheckPlanification();

            if (currentPlan.FoundPlan)
            {
                if (currentStep != -1)
                {
                    switch (currentPlan.GetActionIndex(currentStep))
                    {
                        case 0:
                        {



                            break;
                        }
                    }
                }
            }
        }

        private void CheckPlanification()
        {
            if (planification is null)
            {
                if (nextPlanificationAt <= Time.time)
                {
                    nextPlanificationAt = Time.time + timeBetweenPlanifications;
                    planification = inProgressPlan
                        .Plan(new BossState(this), actions
#if UNITY_EDITOR
                        , log ? e => builder.AppendLine(e) : null
#endif
                        ).CompleteGoal(goal)
                        .WithTimeSlice(1000 / 60)
                        .ExecuteCoroutine();
                }
                else
                    return;
            }

            if (planification.MoveNext())
                return;

            planification = null;

            Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> tmp = currentPlan;
            currentPlan = inProgressPlan;
            inProgressPlan = tmp;

            if (currentPlan.FoundPlan)
            {
                currentStep = 0;
#if UNITY_EDITOR
                if (log)
                {
                    Debug.Log(builder.ToString());
                    builder.Clear();
                }
#endif
            }
            else
            {
                currentStep = -1;
                nextPlanificationAt = Time.time + timeBetweenPlanifications;
                planification = inProgressPlan
                    .Plan(new BossState(this), actions, e => builder.Append(e))
                    .CompleteGoal(goal)
                    .WithTimeSlice(1000 / 60)
                    .ExecuteCoroutine();
            }
        }

        private bool IsTooHurt(BossState state) => (state.BossHealth / (float)lifes) <= tooHurtFactor;
    }
}