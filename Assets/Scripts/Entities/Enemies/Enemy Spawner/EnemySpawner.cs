using Asteroids.Scene;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;
using Enderlook.Unity.Prefabs.HealthBarGUI;
using Enderlook.Unity.Serializables.Ranges;

using System.Collections;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Asteroids.Entities.Enemies
{
    [DefaultExecutionOrder((int)ExecutionOrder.O5_EnemySpawner)]
    public sealed partial class EnemySpawner : MonoBehaviour
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

        [SerializeField, Tooltip("Determines each how many levels the boss is spawned.")]
        private int levelsPerBoss;

        [SerializeField, Tooltip("Prefab of boss.")]
        private Boss bossPrefab;

        [SerializeField, Tooltip("Health bar used by the boss.")]
        private HealthBar bossHealthBar;
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
            EventManager.Subscribe<GameSaver.LoadEvent>(RecalculateEnemyCountManually);

            foreach (EnemyFlyweight template in enemyTemplates)
                template.Initialize();

            Memento.TrackForRewind(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Start()
        {
            StartCoroutine(Work());
            IEnumerator Work()
            {
                yield return new WaitForSeconds(0.5f);
                SpawnBoss();
            }
            return;
            if (remainingEnemies == 0)
                remainingEnemies = SpawnEnemies();
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
                if (GameManager.Level % levelsPerBoss == 0)
                {
                    SpawnBoss();
                    remainingEnemies++;
                }
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

        private void SpawnBoss()
        {
            // Since there is only one kind of boss it doesn't make sense to implement a build pattern (all the configuration is already on the prefab).
            // Since we only spawn the boss once each several levels it doesn't make sense to pool it (the cost of pooling would be larger than creating it again and let the GC collect it).
            // Since we only spawn one boss at the time (hence they are rare) it doesn't make sense to create a factory of them (we only need to instantiate it and set it position and rotation, nothing more).

            Boss boss = Instantiate(bossPrefab);
            boss.SetHealthBar(bossHealthBar);
            boss.transform.position = GetSpawnPosition();
        }

        private void RecalculateEnemyCountManually()
        {
            remainingEnemies = 0;
            GameObject[] objects = FindObjectsOfType<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].activeSelf && objects[i].layer == layer)
                    remainingEnemies++;
            }
            if (FindObjectOfType<Boss>() != null)
                remainingEnemies++;
        }

        private void SpawnEnemy()
        {
            Vector2 position = GetSpawnPosition();

            Vector2 target = (Vector2)camera.ViewportToWorldPoint(new Vector3(Random.value, Random.value));
            Vector2 speed = (position - (target * .8f)).normalized * initialSpeed.Value;

            enemyTemplates.RandomPick().GetFactory().Create((position, -speed));
        }

        private Vector2 GetSpawnPosition()
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

            return camera.ViewportToWorldPoint(positionViewport);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelsPerBoss <= 0)
                Debug.LogError($"{nameof(levelsPerBoss)} can't be 0 nor negative.");
        }
#endif
    }
}