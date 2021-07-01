using Asteroids.Entities.Player;
using Asteroids.PowerUps;
using Asteroids.Scene;

using Enderlook.GOAP;
using Enderlook.StateMachine;
using Enderlook.Unity.Prefabs.HealthBarGUI;

using System;
using System.Text;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D)), RequireComponent(typeof(BossShooter)), DefaultExecutionOrder(100)]
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
        [SerializeField, Min(0), Tooltip("Required time to update plan.")]
        private float timeBetweenPlanifications;

        [SerializeField, Tooltip("Close range gameobject.")]
        private GameObject closeRange;
#pragma warning restore CS0649

        private new Rigidbody2D rigidbody;
        private new Collider2D collider;
        private new SpriteRenderer renderer;
        private HealthBar healthBar;

        private int currentLifes;
        private float invulnerabilityTime;

        private Plan<IGoal<BossState>, NodeBase> currentPlan = new Plan<IGoal<BossState>, NodeBase>();
        private Plan<IGoal<BossState>, NodeBase> inProgressPlan = new Plan<IGoal<BossState>, NodeBase>();
        private int currentStep;

        private readonly NodeBase[] actions = new NodeBase[6];
        private readonly PlayerIsDeadGoal goal = new PlayerIsDeadGoal();

        private float requiredDistanceToPlayerForCloseAttack;
        public const float FurtherDistanceToPlayer = 9;
        private const float CloseAttackDuration = .8f;
        private const float AverageTimeRequiredByFarAttack = 2f;

        private PlanningCoroutine<IGoal<BossState>, NodeBase> planification;
        private float nextPlanificationAt;
#if UNITY_EDITOR
        private StringBuilder builder = new StringBuilder();
        private StringBuilder builder2 = new StringBuilder();
        private const bool log = false;
#endif

        private StateMachine<object, object, object> machine;
        private static readonly object auto = new object();

        private const int AttackCloseIndex = 0;
        private const int AttackFarIndex = 1;
        private const int PickUpPowerUpIndex = 4;
        private const int WaitForPowerUpSpawn = 5;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();
            healthBar = FindObjectOfType<HealthBar>();

            currentLifes = lifes;
            Debug.Assert(closeRange.GetComponent<Collider2D>() != null);
            Vector2 size = closeRange.GetComponent<Collider2D>().bounds.size;
            requiredDistanceToPlayerForCloseAttack = Mathf.Max(size.x, size.y);

            // Also, each event has transitions to any other event, since the recalculation of a GOAP can completely change the current plan.
            // Finally, we need an additional states (and event) used when plan is being calculated, for that reason we use a dummy object.
            // That is why whe use `object` as the generic parameters of the state machine.
            StateMachineBuilder<object, object, object> builder = StateMachine<object, object, object>.Builder();

            StateBuilder<object, object, object>[] builders = new StateBuilder<object, object, object>[7];
            CreateAndAddAttackCloseAbility(builder, builders, AttackCloseIndex);
            CreateAndAddAttackFarAbility(builder, builders, AttackFarIndex);
            CreateAndAddGetCloserAbility(builder, builders, 2);
            CreateAndAddGetFurtherAbility(builder, builders, 3);
            CreateAndAddPickUpPowerUpAbility(builder, builders, PickUpPowerUpIndex);
            CreateAndAddWaitForPowerUpSpawnAbility(builder, builders, WaitForPowerUpSpawn);
            builders[6] = builder.In(auto).OnUpdate(Improvise);

            // Since plans can be modified at any moment, all states requires transitions to all other states.
            for (int i = 0; i < builders.Length; i++)
            {
                StateBuilder<object, object, object> builder_ = builders[i];
                for (int j = 0; j < actions.Length; j++)
                {
                    IAction<BossState, IGoal<BossState>> action = actions[j];
                    // We use the say key for action and state due to its 1 : 1 mapping.
                    builder_.On(action).Goto(action);
                }
                // We use the say key for action and state due to its 1 : 1 mapping.
                builder_.On(auto).Goto(auto);
            }

            machine = builder.SetInitialState(auto).Build();
            machine.Start();

            currentStep = -1;
            CheckPlanification();

            EventManager.Subscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);

            // For gameplay reasons the boss is not tracked by the rewind feature.
        }

        public void SetHealthBar(HealthBar healthBar)
        {
            this.healthBar = healthBar;
            healthBar.gameObject.SetActive(true);
            healthBar.ManualUpdate(lifes, currentLifes);
        }

        private void Improvise()
        {
            // We only update on this state if the planner hasn't give us a plan yet (it's working on it).
            // So we improvise.

            if (IsTooHurt())
            {
                if (FindObjectOfType<PowerUpTemplate.PickupBehaviour>() != null)
                {
                    machine.Fire(actions[PickUpPowerUpIndex]);
                    return;
                }
                else if (PowerUpManager.TimeSinceLastSpawnedPowerUp < PowerUpManager.SpawnTime / 3)
                {
                    machine.Fire(actions[WaitForPowerUpSpawn]);
                    return;
                }
            }

            float distance = Vector3.Distance(PlayerController.Position, transform.position);
            if (distance <= requiredDistanceToPlayerForCloseAttack)
                machine.Fire(actions[AttackCloseIndex]);
            else
                machine.Fire(actions[AttackFarIndex]);
        }

        private void FixedUpdate()
        {
            if (invulnerabilityTime > 0)
            {
                if (invulnerabilityTime <= Time.time)
                {
                    collider.isTrigger = false;
                    renderer.color = Color.white;
                }
            }

            CheckPlanification();
            machine.Update();
        }

        private void OnPowerUpPicked(OnPowerUpPickedEvent @event)
        {
            if (!@event.PickedByPlayer)
            {
                currentLifes = Mathf.Min(currentLifes + healthRestoredPerPowerUp, lifes);
                healthBar.UpdateValues(lifes);
                if (machine.State == actions[PickUpPowerUpIndex])
                    Next();
            }
        }

        private void RotateTowardsDirection(Vector3 direction)
        {
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90;
            rigidbody.rotation = Mathf.MoveTowardsAngle(rigidbody.rotation, angle, rotationSpeed * Time.deltaTime);
        }

        private void MoveAndRotateTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            RotateTowardsDirection(direction);
            rigidbody.position = Vector3.MoveTowards(rigidbody.position, target, movementSpeed * Time.deltaTime);
        }

        public void MoveAndRotateTowards(Vector3 target, float distance = 0)
        {
            Vector3 direction = (target - transform.position).normalized;
            RotateTowardsDirection(direction);
            Vector3 newPosition = Vector3.MoveTowards(rigidbody.position, target, movementSpeed * Time.deltaTime);
            if (Vector3.Distance(newPosition, target) < distance)
            {
                if (distance > direction.magnitude)
                    return;
                newPosition = target + (direction * distance);
            }
            rigidbody.position = newPosition;
        }

        private void MoveAndRotateAway(Vector3 target)
        {
            Vector3 direction = (transform.position - target).normalized;
            RotateTowardsDirection(direction);
            rigidbody.position += (Vector2)direction * movementSpeed * Time.deltaTime;
        }

        public void Next()
        {
            if (currentPlan.FoundPlan && ++currentStep < currentPlan.GetActionsCount())
            {
                machine.Fire(currentPlan.GetAction(currentStep));
                return;
            }

            currentStep = -1;
            machine.Fire(auto);

            if (planification is null)
                Planify();
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

            Plan<IGoal<BossState>, NodeBase> tmp = currentPlan;
            currentPlan = inProgressPlan;
            inProgressPlan = tmp;

            if (currentPlan.FoundPlan)
            {
                currentStep = 0;
#if UNITY_EDITOR
                if (log)
                {
                    int stepsCount = currentPlan.GetStepsCount();
                    if (stepsCount > 0)
                        for (int i = 0; i < stepsCount; i++)
                            builder2.AppendLine(currentPlan.GetAction(i).ToString());
                    builder.Insert(0, builder2);
                    builder2.Clear();

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
                        , log ? new Action<string>(e => builder.AppendLine(e)) : null
#endif
                        ).CompleteGoal(goal)
                .WithTimeSlice(1000 / 60)
                .ExecuteCoroutine();
        }

        private bool IsTooHurt(BossState state) => (state.BossHealth / (float)lifes) <= tooHurtFactor;

        private bool IsTooHurt() => (currentLifes / (float)lifes) <= tooHurtFactor;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<PlayerController>() != null)
                return;

            if (lifes == 1)
            {
                EventManager.Raise(new EnemySpawner.EnemyDestroyedEvent("Boss", scoreWhenDestroyed));
                Destroy(gameObject);
            }
            else
            {
                lifes--;
                healthBar.UpdateValues(lifes);
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
            if (healthBar != null) // On returning from scene, the gameobject may be destroyed before this one.
                healthBar.gameObject.SetActive(false);
            EventManager.Unsubscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);
            // Forcing an state transition will call OnExit of current frame, removing possible memory leak from subscribed events.
            machine.Fire(auto);
        }
    }
}