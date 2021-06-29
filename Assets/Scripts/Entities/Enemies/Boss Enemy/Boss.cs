using Asteroids.PowerUps;
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
        [SerializeField, Min(0), Tooltip("Required time to update plan.")]
        private float timeBetweenPlanifications;

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

        private float requiredDistanceToPlayerForCloseAttack;
        private const float FurtherDistanceToPlayer = 15;
        private const float CloseAttackDuration = .8f;
        private const float AverageTimeRequiredByFarAttack = 2f;

        private PlanningCoroutine<IGoal<BossState>, IAction<BossState, IGoal<BossState>>> planification;
        private float nextPlanificationAt;
#if UNITY_EDITOR
        private StringBuilder builder = new StringBuilder();
        private const bool log = true;
#endif

        private StateMachine<object, object, object> machine;
        private static readonly object auto = new object();

        private const int PickUpPowerUpStateIndex = 4;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();

            currentLifes = lifes;

            Rect rect = closeRange.GetComponent<SpriteRenderer>().sprite.rect;
            requiredDistanceToPlayerForCloseAttack = Mathf.Max(rect.width, rect.height) / 1.75f;

            // Also, each event has transitions to any other event, since the recalculation of a GOAP can completely change the current plan.
            // Finally, we need an additional states (and event) used when plan is being calculated, for that reason we use a dummy object.
            // That is whay whe use `object` as the generic parameters of the state machine.
            StateMachineBuilder<object, object, object> builder = StateMachine<object, object, object>.Builder();

            StateBuilder<object, object, object>[] builders = new StateBuilder<object, object, object>[7];
            CreateAndAddAttackCloseAbility(builder, builders, 0);
            CreateAndAddAttackFarAbility(builder, builders, 1);
            CreateAndAddGetCloserAbility(builder, builders, 2);
            CreateAndAddGetFurtherAbility(builder, builders, 3);
            CreateAndAddPickUpPowerUpAbility(builder, builders, PickUpPowerUpStateIndex);
            CreateAndAddWaitForPowerUpSpawnAbility(builder, builders, 5);
            builders[6] = builder.In(auto);

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
            //CheckPlanification();

            

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
                if (machine.State == actions[PickUpPowerUpStateIndex])
                    Next();
            }
        }

        private void MoveAndRotateTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90;
            rigidbody.rotation = Mathf.MoveTowardsAngle(rigidbody.rotation, angle, rotationSpeed * Time.deltaTime);
            rigidbody.position = Vector3.MoveTowards(rigidbody.position, target, movementSpeed * Time.deltaTime);
        }

        private void MoveAndRotateAway(Vector3 target)
        {
            Vector3 direction = (transform.position - target).normalized;
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90;
            rigidbody.rotation = Mathf.MoveTowardsAngle(rigidbody.rotation, angle, rotationSpeed * Time.deltaTime);
            rigidbody.position += (Vector2)direction * movementSpeed * Time.deltaTime;
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