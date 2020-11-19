using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    public partial class PlayerController : MonoBehaviour
    {
        private new Rigidbody2D rigidbody;
        private new Collider2D collider;

        private int lifes;
        private int scoreToNextLife;
        private float invulnerabilityTime;

        private PlayerModel model;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            model = GetComponent<PlayerModel>();

            lifes = model.startingLifes;
            scoreToNextLife = model.scorePerLife;

            EventManager.Subscribe<GameManager.ScoreHasChangedEvent>(OnScoreChanged);

            Memento.TrackForRewind(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
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