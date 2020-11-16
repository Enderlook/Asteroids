using Asteroids.Events;

using Enderlook.Unity.Components.ScriptableSound;
using Enderlook.Unity.Extensions;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Collider2D))]
    public class Player : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Amount of lifes the player start with.")]
        private int startingLifes;

        [SerializeField, Tooltip("Maximum amount of lifes the player can have.")]
        private int maxLifes;

        [SerializeField, Tooltip("Duration of invulnerability after lose a life.")]
        private float invulnerabilityDuration;

        [SerializeField, Min(1), Tooltip("Amount of points required to earn a new life. Up to a maximum of one life can be get per score increase.")]
        private int scorePerLife;

        [SerializeField, Tooltip("Sound played on death.")]
        private SimpleSoundPlayer deathSound;

        [SerializeField, Tooltip("Sound played on get new life.")]
        private SimpleSoundPlayer newLife;

        [SerializeField, Tooltip("Layer to hit.")]
        private LayerMask layerToHit;
#pragma warning restore CS0649

        private static Player instance;

        public static int StartingLifes => instance.startingLifes;

        public static int MaxLifes => instance.maxLifes;

        private int lifes;

        private new Rigidbody2D rigidbody;

        private new Collider2D collider;

        private int scoreToNextLife;

        private float invulnerabilityTime;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(Player)} can't have more than one instance at the same time.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();

            lifes = startingLifes;
            scoreToNextLife = scorePerLife;

            EventManager.Subscribe<ScoreHasChangedEvent>(OnScoreChanged);

            GlobalMementoManager.Subscribe(CreateMemento, ConsumeMemento);

            (Vector3 position, float rotation, Vector2 velocity, float angularVelocity) CreateMemento()
            {
                // The following features are not tracked by the memento for gameplay reasons:
                // - Lifes
                // - Invulnerability time
                // - Score

                Vector3 position = rigidbody.position;
                float rotation = rigidbody.rotation;
                Vector2 velocity = rigidbody.velocity;
                float angularVelocity = rigidbody.angularVelocity;

                // The memento object is simple, so we store it as a tuple
                return (position, rotation, velocity, angularVelocity);
            }

            void ConsumeMemento((Vector3 position, float rotation, Vector2 velocity, float angularVelocity) memento)
            {
                rigidbody.position = memento.position;
                rigidbody.rotation = memento.rotation;
                rigidbody.velocity = memento.velocity;
                rigidbody.angularVelocity = memento.angularVelocity;

                // We always become the player invulnerable
                BecomeInvulnerable();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (invulnerabilityTime > 0)
            {
                invulnerabilityTime -= Time.deltaTime;
                if (invulnerabilityTime <= 0)
                    collider.enabled = true;
            }
        }

        private void OnScoreChanged(ScoreHasChangedEvent @event)
        {
            if (scoreToNextLife <= @event.NewScore)
            {
                newLife.Play();
                scoreToNextLife += scorePerLife;
                AddNewLife();
            }
        }

        public void AddNewLife()
        {
            lifes++;
            EventManager.Raise(PlayerHealthChangedEvent.Increase);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collider.gameObject.layer != (layerToHit | (1 << collider.gameObject.layer)))
                return;

            deathSound.Play();

            if (lifes == 0)
                EventManager.Raise(LevelTerminationEvent.Lose);
            else
            {
                lifes--;
                EventManager.Raise(PlayerHealthChangedEvent.Decrease);
            }

            rigidbody.position = Vector2.zero;
            rigidbody.rotation = 0;
            rigidbody.velocity = default;

            BecomeInvulnerable();
        }

        private void BecomeInvulnerable()
        {
            collider.enabled = false;
            invulnerabilityTime = invulnerabilityDuration;
        }
    }
}
