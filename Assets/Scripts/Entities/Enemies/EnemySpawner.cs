using Asteroids.Events;
using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Enumerables;
using Enderlook.Unity.Serializables.Ranges;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Asteroids.Entities.Enemies
{
    [DefaultExecutionOrder((int)ExecutionOrder.O5_EnemySpawner)]
    public class EnemySpawner : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Initial amount of enemies.")]
        private int initialEnemyAmount;

        [SerializeField, Tooltip("Additional amount of enemies per level.")]
        private int additionalEnemiesPerLevel;

        [SerializeField, Tooltip("Maximum amount of enemies per level.")]
        private int maximumAmountOfEnemies;

        [SerializeField, Tooltip("Possible enemies to spawn.")]
        private EnemyBuilderData[] enemyBuilders;

        [SerializeField, Tooltip("Random speed of spawned enemies.")]
        private RangeFloat initialSpeed;
#pragma warning restore CS0649

        private Pool<EnemyBuilderData.EnemyHandler, EnemyBuilderData> pool;

        private new Camera camera;

        private int remainingEnemies;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            pool = new Pool<EnemyBuilderData.EnemyHandler, EnemyBuilderData>(
                (configuration) => configuration.Create(),
                (enemy, configuration) => configuration.Initialize(enemy),
                (enemy) => enemy.Deinitialize()
            );

            camera = Camera.main;

            EventManager.Subscribe<LevelTerminationEvent>(OnLevelTermination);
            EventManager.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);

            remainingEnemies = SpawnEnemies();
        }

        private void OnLevelTermination(LevelTerminationEvent @event)
        {
            if (@event.HasWon)
                remainingEnemies = SpawnEnemies();
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent @event)
        {
            pool.Store(@event.Enemy);
            remainingEnemies--;
            if (remainingEnemies == 0)
                EventManager.Raise(LevelTerminationEvent.Win);
        }

        private int SpawnEnemies()
        {
            int maxEnemies = Mathf.Min(initialEnemyAmount + ((GameManager.Level - 1) * additionalEnemiesPerLevel), maximumAmountOfEnemies);
            for (int i = 0; i < maxEnemies; i++)
                SpawnEnemy();
            return maxEnemies;
        }

        private void SpawnEnemy()
        {
            EnemyBuilderData configuration = enemyBuilders.RandomPick();
            EnemyBuilderData.EnemyHandler enemy = pool.Get(configuration);

            Vector3 position;
            if (Random.value > .5)
                position = new Vector3(1.05f, Random.value, 0);
            else
                position = new Vector3(-0.05f, Random.value, 0);

            enemy.Initialize(camera.ViewportToWorldPoint(position), new Vector2(Random.value, Random.value), initialSpeed.Value);
        }
    }
}