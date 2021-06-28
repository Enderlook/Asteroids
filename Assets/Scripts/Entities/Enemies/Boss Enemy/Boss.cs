﻿using Asteroids.PowerUps;
using Asteroids.Scene;

using Enderlook.GOAP;
using Enderlook.StateMachine;

using System;
using System.Text;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [DefaultExecutionOrder(100)]
    public sealed partial class Boss : MonoBehaviour
    {
#pragma warning disable CS0649
        [Header("Health")]
        [SerializeField, Tooltip("Maximum life.")]
        private int lifes;

        [SerializeField, Range(0, 1), Tooltip("Life factor considered too hurt.")]
        private float tooHurtFactor;

        [SerializeField, Min(1), Tooltip("Health restored per power up.")]
        private int healthRestoredPerPowerUp;

        [Header("Movement")]
        [SerializeField, Tooltip("Movement speed.")]
        private float movementSpeed;

        [SerializeField, Tooltip("Rotation speed.")]
        private float rotationSpeed;

        [Header("Invulnerability")]
        [SerializeField, Tooltip("How many seconds does invulnerability last.")]
        private float invulnerabilityDuration;

        [SerializeField, Tooltip("Coor tone applied when invulnerable.")]
        private Color invulnerabilityColor;

        [Header("Death")]
        [SerializeField, Tooltip("Points got when this enemy is destroyed.")]
        private int scoreWhenDestroyed;

        [Header("Setup")]
        [SerializeField, Tooltip("Close range gameobject.")]
        private GameObject closeRange;
#pragma warning restore CS0649

        private new Rigidbody2D rigidbody;
        private new Collider2D collider;
        private new SpriteRenderer renderer;

        private int currentLifes;
        private float invulnerabilityTime;

        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> currentPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> inProgressPlan = new Plan<IGoal<BossState>, IAction<BossState, IGoal<BossState>>>();
        private int currentStep;

        private readonly IAction<BossState, IGoal<BossState>>[] actions = new IAction<BossState, IGoal<BossState>>[6];
        private readonly PlayerIsDeadGoal goal = new PlayerIsDeadGoal();

        private const float ClosestDistanceToPlayer = 1;
        private const float FurtherDistanceToPlayer = 15;
        private const float CloseAttackDuration = .8f;
        private const float AverageTimeRequiredByFarAttack = 2f;

        private PlanningCoroutine<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> planification;
        private float nextPlanificationAt;
        private const float timeBetweenPlanifications = 2f;

        private StateMachine<object, object, object> machine;
        private static readonly object auto = new object();

#if UNITY_EDITOR
        private StringBuilder builder = new StringBuilder();
        private const bool log = true;
#endif

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();

            currentLifes = lifes;

            AttackCloseAction attackCloseAction = (AttackCloseAction)(actions[0] = new AttackCloseAction(this));
            AttackFarAction attackFarAction = (AttackFarAction)(actions[1] = new AttackFarAction(this));
            GetCloserPlayerAction getCloserPlayerAction = (GetCloserPlayerAction)(actions[2] = new GetCloserPlayerAction(this));
            GetFurtherPlayerAction getFurtherPlayerAction = (GetFurtherPlayerAction)(actions[3] = new GetFurtherPlayerAction(this));
            PickPowerUpAction pickPowerUpAction = (PickPowerUpAction)(actions[4] = new PickPowerUpAction(this));
            WaitForPowerUpAction waitForPowerUpAction = (WaitForPowerUpAction)(actions[5] = new WaitForPowerUpAction(this));

            // Each event has a 1 : 1 mapping to an state, for that reason, we use the action themselves as both event and states.
            // Also, each event has transitions to any other event, since the recalculation of a GOAP can completely change the current plan.
            // Finally, we need a few additional states and/or event used when plan is being calculated or power up is picked, for that reason we use a dummy object.
            // That is whay whe use `object` as the generic parameters of the state machine.
            StateMachineBuilder<object, object, object> builder = StateMachine<object, object, object>
                 .Builder()
                 .SetInitialState(pickPowerUpAction);

            SetNode(attackCloseAction);
            //SetNode(attackFarAction);
            //SetNode(getCloserPlayerAction);
            //SetNode(getFurtherPlayerAction);
            SetNode(pickPowerUpAction);
            SetNode(waitForPowerUpAction);

            machine = builder.Build();
            machine.Start();

            currentStep = -1;
            //CheckPlanification();

            void SetNode(IFSMState node) => builder
                    .In(node)
                        .OnEntry(node.OnEntry)
                        .OnExit(node.OnExit)
                        .OnUpdate(node.OnUpdate)
                        .On(attackCloseAction).Goto(attackCloseAction)
                        /*.On(attackFarAction).Goto(attackFarAction)*/
                        /*.On(getCloserPlayerAction).Goto(getCloserPlayerAction)*/
                        /*.On(getFurtherPlayerAction).Goto(getFurtherPlayerAction)*/
                        .On(pickPowerUpAction).Goto(pickPowerUpAction)
                        .On(waitForPowerUpAction).Goto(waitForPowerUpAction);
                        //.On(auto).Goto(auto);

            EventManager.Subscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);

            // For gameplay reasons the boss is not tracked by the rewind feature.
        }

        private void Update()
        {
            if (invulnerabilityTime > 0)
            {
                if (invulnerabilityTime <= Time.time)
                {
                    collider.isTrigger = false;
                    renderer.color = Color.white;
                }
            }

            //CheckPlanification();
            machine.Update();
        }

        private void OnPowerUpPicked(OnPowerUpPickedEvent @event)
        {
            if (!@event.PickedByPlayer)
            {
                currentLifes = Mathf.Min(currentLifes + healthRestoredPerPowerUp, lifes);
                Next();
            }
        }

        private void MoveAndRotateTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            rigidbody.rotation = Mathf.MoveTowardsAngle(rigidbody.rotation, angle, rotationSpeed * Time.deltaTime);
            rigidbody.position = Vector3.MoveTowards(rigidbody.position, target, movementSpeed * Time.deltaTime);
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
            {
                currentStep = -1;
                machine.Fire(auto);
            }
        }

        private void WorldIsNotAsExpected()
        {
            if (planification is null)
                Planify();
            machine.Fire(auto);
        }

        private void CheckPlanification()
        {
            if (planification is null)
            {
                if (nextPlanificationAt <= Time.time)
                    Planify();
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
            Planify();
        }

        private void Planify()
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

        private bool IsTooHurt(BossState state) => (state.BossHealth / (float)lifes) <= tooHurtFactor;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (lifes == 1)
                EventManager.Raise(new EnemySpawner.EnemyDestroyedEvent(name, scoreWhenDestroyed));
            else
            {
                lifes--;
                BecomeInvulnerable();
            }
        }

        private void BecomeInvulnerable()
        {
            collider.isTrigger = true;
            invulnerabilityTime = invulnerabilityDuration;
            renderer.color = invulnerabilityColor;
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);
            // Forcing an sate transition will call OnExit of current frame, removing possible memory leak from subscribed events.
            machine.Fire(auto);
        }
    }
}