using Asteroids.Scene;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Collider2D)), RequireComponent(typeof(SpriteRenderer))]
    public partial class Player : MonoBehaviour
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

        public static int Lifes => instance.lifes;

        public static int MaxLifes => instance.maxLifes;

        private int lifes;

        private new Rigidbody2D rigidbody;

        private new Collider2D collider;

        private int scoreToNextLife;

        private float invulnerabilityTime;

        private new SpriteRenderer renderer;

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

            EventManager.Subscribe<GameManager.ScoreHasChangedEvent>(OnScoreChanged);

            Memento.TrackForRewind(this);

            GameSaver.SubscribePlayer(() => new State(this), (state) => state.Load(this));
        }

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

        private void OnScoreChanged(GameManager.ScoreHasChangedEvent @event)
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
            EventManager.Raise(HealthChangedEvent.Increase);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            deathSound.Play();

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
            invulnerabilityTime = invulnerabilityDuration;
        }
    }
}
