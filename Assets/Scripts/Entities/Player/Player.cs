using Asteroids.Events;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;
using Enderlook.Unity.Extensions;

using System;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Collider2D)), RequireComponent(typeof(SpriteRenderer))]
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
#pragma warning restore CS0649

        private static Player instance;

        public static int StartingLifes => instance.startingLifes;

        public static int MaxLifes => instance.maxLifes;

        private int lifes;

        private new Rigidbody2D rigidbody;

        private new Collider2D collider;

        private int scoreToNextLife;

        private float invulnerabilityTime;

        private SpriteRenderer renderer;

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
            renderer = GetComponent<SpriteRenderer>();

            lifes = startingLifes;
            scoreToNextLife = scorePerLife;

            EventManager.Subscribe<ScoreHasChangedEvent>(OnScoreChanged);
            EventManager.Subscribe<StartRewindEvent>(OnStartRewind);
            EventManager.Subscribe<StopRewindEvent>(OnStopRewind);

            GlobalMementoManager.Subscribe(CreateMemento, ConsumeMemento, interpolateMementos);

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

            void ConsumeMemento((Vector3 position, float rotation, Vector2 velocity, float angularVelocity)? memento)
            {
                if (memento.HasValue)
                {
                    (Vector3 position, float rotation, Vector2 velocity, float angularVelocity) memento_ = memento.Value;
                    rigidbody.position = memento_.position;
                    rigidbody.rotation = memento_.rotation;
                    rigidbody.velocity = memento_.velocity;
                    rigidbody.angularVelocity = memento_.angularVelocity;
                }
            }
        }

        private static readonly Func<(Vector3 position, float rotation, Vector2 velocity, float angularVelocity), (Vector3 position, float rotation, Vector2 velocity, float angularVelocity), float, (Vector3 position, float rotation, Vector2 velocity, float angularVelocity)> interpolateMementos = InterpolateMementos;

        private static (Vector3 position, float rotation, Vector2 velocity, float angularVelocity) InterpolateMementos(
                (Vector3 position, float rotation, Vector2 velocity, float angularVelocity) a,
                (Vector3 position, float rotation, Vector2 velocity, float angularVelocity) b,
                float delta
            )
        {
            // Handle screen wrapping
            float height = Camera.main.orthographicSize * 2;
            height *= .9f; // Allow offset error
            if (Mathf.Abs(a.position.y - b.position.y) > height || Mathf.Abs(a.position.x - b.position.x) > height * Camera.main.aspect)
                return delta > .5f ? b : a;

            return (
             Vector3.Lerp(a.position, b.position, delta),
             Mathf.Lerp(a.rotation, b.rotation, delta),
             Vector2.Lerp(a.velocity, b.velocity, delta),
             Mathf.Lerp(a.angularVelocity, b.angularVelocity, delta)
         );
        }

        private void OnStartRewind(StartRewindEvent @event) => collider.enabled = false;

        private void OnStopRewind(StopRewindEvent @event) => collider.enabled = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (!GlobalMementoManager.IsRewinding && invulnerabilityTime > 0)
            {
                invulnerabilityTime -= Time.deltaTime;
                if (invulnerabilityTime <= 0)
                {
                    collider.enabled = true;
                    renderer.color = Color.white;
                }
                else
                    renderer.color = new Color(.5f, .5f, .5f);
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
