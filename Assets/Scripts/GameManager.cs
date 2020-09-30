using Asteroids.Events;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O4_GameManager)]
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static int Level => instance.level;

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
            EventManager.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void OnLevelComplete() => level++;

        private void OnEnemyDestroyed(EnemyDestroyedEvent @event)
        {
            score += @event.Score;
            EventManager.Raise(new ScoreHasChangedEvent(score));
        }
    }
}