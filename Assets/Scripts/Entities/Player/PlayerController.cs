using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        private PlayerModel model;

        private new Rigidbody2D rigidbody;
        private new Collider2D collider;

        private int scoreToNextLife;
        private float invulnerabilityTime;

        public int Lifes { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            model = GetComponent<PlayerModel>();

            Lifes = model.startingLifes;
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
                    collider.enabled = true;
                    EventManager.Raise(VulnerabilityChangedEvent.Vulnerable);
                }
            }

            if (GlobalMementoManager.IsRewinding)
                return;

            Move();
            Rotate();
        }

        private void Move()
        {
            rigidbody.AddRelativeForce(Vector2.up * Mathf.Max(Input.GetAxis("Vertical"), 0) * model.accelerationSpeed, ForceMode2D.Force);
            rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, model.maximumSpeed);
        }

        private void Rotate() => rigidbody.SetRotation(rigidbody.rotation - Input.GetAxis("Horizontal") * model.rotationSpeed * Time.deltaTime);

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
            if (Lifes >= model.maxLifes)
                return;
            Lifes++;
            EventManager.Raise(HealthChangedEvent.IncreaseByScore);
        }

        public void AddNewLifeByPowerUp()
        {
            if (Lifes >= model.maxLifes)
                return;
            Lifes++;
            EventManager.Raise(HealthChangedEvent.IncreaseByPowerUp);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Lifes == 0)
                EventManager.Raise(GameManager.LevelTerminationEvent.Lose);
            else
            {
                Lifes--;
                EventManager.Raise(HealthChangedEvent.Decrease);
            }

            rigidbody.position = Vector2.zero;
            rigidbody.rotation = 0;
            rigidbody.velocity = default;

            BecomeInvulnerable();
        }
        private void BecomeInvulnerable()
        {
            collider.enabled = false;
            invulnerabilityTime = model.invulnerabilityDuration;
            EventManager.Raise(VulnerabilityChangedEvent.Invulnerable);
        }
    }

}