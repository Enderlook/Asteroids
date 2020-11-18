using Asteroids.Entities.Enemies;
using Asteroids.UI;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O4_GameManager)]
    public partial class GameManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Panel shown on loose.")]
        private GameObject gameOver;
#pragma warning restore CS0649

        private static GameManager instance;

        public static int Level => instance.level;

        public static int Score => instance.score;

        private int level = 1;

        private int score;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(GameManager)} can't have more than one instance at the same time.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            EventManager.Subscribe<LevelTerminationEvent>(OnLevelComplete);
            EventManager.Subscribe<EnemySpawner.EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void OnLevelComplete(LevelTerminationEvent @event)
        {
            if (@event.HasWon)
                level++;
            else
            {
                FindObjectOfType<PauseManager>().Pause();
                gameOver.SetActive(true);
            }
        }

        private void OnEnemyDestroyed(EnemySpawner.EnemyDestroyedEvent @event)
        {
            score += @event.Score;
            EventManager.Raise(new ScoreHasChangedEvent(score));
        }
    }
}