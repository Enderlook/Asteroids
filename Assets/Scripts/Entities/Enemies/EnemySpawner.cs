using Asteroids.Scene;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;
using Enderlook.Unity.Serializables.Ranges;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Asteroids.Entities.Enemies
{
    [DefaultExecutionOrder((int)ExecutionOrder.O5_EnemySpawner)]
    public partial class EnemySpawner : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Initial amount of enemies.")]
        private int initialEnemyAmount;

        [SerializeField, Tooltip("Additional amount of enemies per level.")]
        private int additionalEnemiesPerLevel;

        [SerializeField, Tooltip("Maximum amount of enemies per level.")]
        private int maximumAmountOfEnemies;

        public int MaxmiumAmountOfEnemies => maximumAmountOfEnemies;

        [SerializeField, Tooltip("Possible enemies to spawn."), Expandable]
        private EnemyFlyweight[] enemyTemplates;

        [SerializeField, Tooltip("Random speed of spawned enemies.")]
        private RangeFloat initialSpeed;

        [SerializeField, Tooltip("Sound played when spawning new enemies.")]
        private SimpleSoundPlayer spawnSound;

        [SerializeField, Layer, Tooltip("Layer of enemies, used to count them.")]
        private int layer;
#pragma warning restore CS0649

        private new Camera camera;

        private int remainingEnemies;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            camera = Camera.main;

            EventManager.Subscribe<GameManager.LevelTerminationEvent>(OnLevelTermination);
            EventManager.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            EventManager.Subscribe<SplitEnemyBuilder.EnemySplittedEvent>(OnEnemySplitted);

            foreach (EnemyFlyweight template in enemyTemplates)
                template.Initialize();

            remainingEnemies = SpawnEnemies();

            Memento.TrackForRewind(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (!GlobalMementoManager.IsRewinding)
            {
                if (remainingEnemies == 0)
                    EventManager.Raise(GameManager.LevelTerminationEvent.Win);
            }
        }

        private void OnLevelTermination(GameManager.LevelTerminationEvent @event)
        {
            if (@event.HasWon)
            {
                remainingEnemies = SpawnEnemies();
                spawnSound.Play();
            }
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent @event) => remainingEnemies--;

        private void OnEnemySplitted(SplitEnemyBuilder.EnemySplittedEvent @event) => remainingEnemies += @event.Amount;

        private int SpawnEnemies()
        {
            int maxEnemies = Mathf.Min(initialEnemyAmount + ((GameManager.Level - 1) * additionalEnemiesPerLevel), maximumAmountOfEnemies);
            for (int i = 0; i < maxEnemies; i++)
                SpawnEnemy();
            return maxEnemies;
        }

        private void SpawnEnemy()
        {
            Vector3 positionViewport;

            switch (Random.Range(0, 4))
            {
                case 0:
                    positionViewport = new Vector3(1.05f, Random.value, 0);
                    break;
                case 1:
                    positionViewport = new Vector3(-0.05f, Random.value, 0);
                    break;
                case 2:
                    positionViewport = new Vector3(Random.value, 1.05f, 0);
                    break;
                case 3:
                    positionViewport = new Vector3(Random.value, -0.05f, 0);
                    break;
                default:
                    Debug.LogError("Impossible state.");
                    goto case 0;
            }

            Vector2 position = camera.ViewportToWorldPoint(positionViewport);

            Vector2 speed = (position - new Vector2(Random.value, Random.value)).normalized * initialSpeed.Value;

            enemyTemplates.RandomPick().GetFactory().Create((position, -speed));
        }
    }
}