using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    //IA2-P2
    // ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
    [DefaultExecutionOrder((int)ExecutionOrder.O5_Player)]
#endif
    public sealed partial class PlayerController :
//IA2-P2
// ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
        Spatial.GridEntity
#else
        MonoBehaviour
#endif
    {
        private PlayerModel model;

        private new Rigidbody2D rigidbody;
        private new Collider2D collider;

        private int scoreToNextLife;
        private float invulnerabilityTime;

        private int lifes;

        private static PlayerController instance;

        public static Vector3 Position => instance.transform.position;

        public static int Lifes => instance.lifes;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        //IA2-P2
        // ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
        protected override void Awake()
        {
            base.Awake();
#else
        private void Awake()
        {
#endif

            if (instance != null)
                Debug.LogError("Only a single instance can be found at the same time.");

            instance = this;

            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            model = GetComponent<PlayerModel>();

            lifes = model.startingLifes;
            scoreToNextLife = model.scorePerLife;

            EventManager.Subscribe<GameManager.ScoreHasChangedEvent>(OnScoreChanged);

            Memento.TrackForRewind(this);

            GameSaver.SubscribePlayer(() => new State(this), (state) => state.Load(this));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (invulnerabilityTime > 0)
            {
                invulnerabilityTime -= Time.deltaTime;
                if (invulnerabilityTime <= 0)
                {
                    collider.isTrigger = false;
                    EventManager.Raise(VulnerabilityChangedEvent.Vulnerable);
                }
            }

            if (GlobalMementoManager.IsRewinding)
                return;

            //MYA1-P2
            // ^- Don't touch that comment, used by the teacher
            if (TryGetMoveCommand() is MoveCommand moveCommand)
                moveCommand.Execute();

            if (TryGetRotateCommand() is RotateCommand rotateCommand)
                rotateCommand.Execute();
        }

        private void OnScoreChanged(GameManager.ScoreHasChangedEvent @event)
        {
            if (scoreToNextLife <= @event.NewScore)
            {
                scoreToNextLife += model.scorePerLife;
                AddNewLifeByScore();
            }
        }

        private void AddNewLifeByScore()
        {
            if (lifes >= model.maxLifes)
                return;
            lifes++;
            EventManager.Raise(HealthChangedEvent.IncreaseByScore);
        }

        public void AddNewLifeByPowerUp()
        {
            if (lifes >= model.maxLifes)
                return;
            lifes++;
            EventManager.Raise(HealthChangedEvent.IncreaseByPowerUp);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (lifes == 0)
                EventManager.Raise(GameManager.LevelTerminationEvent.Lose);
            else
            {
                lifes--;
                EventManager.Raise(HealthChangedEvent.Decrease);

                rigidbody.position = Vector2.zero;
                rigidbody.rotation = 0;
                rigidbody.velocity = default;

                BecomeInvulnerable();
            }
        }

        private void BecomeInvulnerable()
        {
            collider.isTrigger = true;
            invulnerabilityTime = model.invulnerabilityDuration;
            EventManager.Raise(VulnerabilityChangedEvent.Invulnerable);
        }
    }

}