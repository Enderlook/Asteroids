using Enderlook.GOAP;
using Enderlook.StateMachine;

using System;
using System.Text;

using UnityEngine;
using UnityEngine.Video;

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

        [SerializeField, Tooltip("Close range gameobject.")]
        private GameObject closeRange;
#pragma warning restore CS0649

        private int currentLifes;

        private const float ClosestDistanceToPlayer = 1;
        private const float FurtherDistanceToPlayer = 15;
        private const int HealthRestoredPerPack = 4;
        private const float CloseAttackDuration = .8f;
        private const float AverageTimeRequiredByFarAttack = 1.4f;

        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> currentPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> inProgressPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private int currentStep;

        private readonly IAction<BossState, IGoal<BossState>>[] actions = new IAction<BossState, IGoal<BossState>>[6];
        private readonly PlayerIsDeadGoal goal = new PlayerIsDeadGoal();

        private PlanningCoroutine<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> planification;
        private float nextPlanificationAt;
        private const float timeBetweenPlanifications = 2f;

        private new Rigidbody2D rigidbody;

        private StateMachine<object, object, object> machine;

#if UNITY_EDITOR
        private StringBuilder builder = new StringBuilder();
        private const bool log = true;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();

            currentLifes = lifes;

            AttackCloseAction attackCloseAction = new AttackCloseAction(this);
            AttackFarAction attackFarAction = new AttackFarAction(this);
            GetCloserPlayerAction getCloserPlayerAction = new GetCloserPlayerAction(this);
            GetFurtherPlayerAction getFurtherPlayerAction = new GetFurtherPlayerAction(this);
            PickPowerUpAction pickPowerUpAction = new PickPowerUpAction(this);
            WaitForPowerUpAction waitForPowerUpAction = new WaitForPowerUpAction();

            object idle = new object();

            /*machine = StateMachine<object, object, object>
                 .Builder()
                 .SetInitialState(attackCloseAction)
                 .In(attackCloseAction)
                    .OnUpdate(() => Debug.Log("HI"))
                 .Build();
            machine.Start();
            return;*/

            // Each event has a 1 : 1 mapping to an state, for that reason, we use the action themselves as both event and states.
            // Also, each event has transitions to any other event, since the recalculation of a GOAP can completely change the current plan.
            // Finally, we need an additional state (and event) used when plan is being calculated, for that reason we use a dummy object.
            // That is whay whe use `object` as the generic parameters of the state machine.
            StateMachineBuilder<object, object, object> builder = StateMachine<object, object, object>
                 .Builder()
                 .SetInitialState(attackCloseAction);

            SetNode(attackCloseAction);

            machine = builder.Build();
            machine.Start();

            actions[0] = attackCloseAction;
            actions[1] = attackFarAction;
            actions[2] = getCloserPlayerAction;
            actions[3] = getFurtherPlayerAction;
            actions[4] = pickPowerUpAction;
            actions[5] = waitForPowerUpAction;

            currentStep = -1;
            //CheckPlanification();

            void SetNode(IFSMState node)
            {
                builder
                    .In(node)
                        .OnEntry(node.OnEntry)
                        .OnExit(node.OnExit)
                        .OnUpdate(node.OnUpdate)
                        .On(attackCloseAction)
                            .Goto(attackCloseAction)
                        /*.On(attackFarAction)
                            .Goto(attackFarAction)
                        .On(getCloserPlayerAction)
                            .Goto(getCloserPlayerAction)
                        .On(getFurtherPlayerAction)
                            .Goto(getFurtherPlayerAction)
                        .On(pickPowerUpAction)
                            .Goto(pickPowerUpAction)
                        .On(waitForPowerUpAction)
                            .Goto(waitForPowerUpAction)*/;
            }

            // For gameplay reasons the boss is not tracked by the rewind feature.
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            //CheckPlanification();
            machine.Update();
        }

        private void Next()
        {
            Debug.Log("NEXT");
            return;

            if (currentPlan.FoundPlan && currentStep < currentPlan.GetActionsCount())
            {
                currentStep++;
                machine.Fire(currentPlan.GetAction(currentStep));
            }
            else
                currentStep = -1;
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
                        , log ? (Action<string>)(e => builder.AppendLine(e)) : null
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
                if (currentPlan.GetActionsCount() > 0)
                {
                    machine.Fire(currentPlan.GetAction(currentStep));
                    return;
                }
            }

            currentStep = -1;
            nextPlanificationAt = Time.time + timeBetweenPlanifications;
            planification = inProgressPlan
                .Plan(new BossState(this), actions, e => builder.Append(e))
                .CompleteGoal(goal)
                .WithTimeSlice(1000 / 60)
                .ExecuteCoroutine();
        }

        private bool IsTooHurt(BossState state) => (state.BossHealth / (float)lifes) <= tooHurtFactor;
    }
}