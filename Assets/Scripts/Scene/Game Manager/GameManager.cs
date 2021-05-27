using Asteroids.Entities.Enemies;
using Asteroids.UI;

using Spatial;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O4_GameManager)]
    public sealed partial class GameManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Panel shown on loose.")]
        private GameObject gameOver;
#pragma warning restore CS0649

        private static GameManager instance;

        //IA2-P2
        // ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
        private static SpatialGrid spatialGrid;
        public static SpatialGrid SpatialGrid => spatialGrid;
#endif

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
            Cursor.visible = false;

            EventManager.Subscribe<LevelTerminationEvent>(OnLevelComplete);
            EventManager.Subscribe<EnemySpawner.EnemyDestroyedEvent>(OnEnemyDestroyed);

            GameSaver.SubscribeGameManager(() => new State(this), (state) => state.Load(this));

            //IA2-P2
            // ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
            spatialGrid = gameObject.AddComponent<SpatialGrid>();
            spatialGrid.x = -15;
            spatialGrid.y = -12;
            spatialGrid.cellWidth = 1.2f;
            spatialGrid.cellHeight = 1;
            spatialGrid.width = 25;
            spatialGrid.height = 25;
            spatialGrid.Generate();
#endif
        }

        private void OnLevelComplete(LevelTerminationEvent @event)
        {
            if (@event.HasWon)
                level++;
            else
            {
                FindObjectOfType<PauseManager>().Pause();
                gameOver.SetActive(true);
                FindObjectOfType<Scoreboard>().OrderScores();
            }
        }

        private void OnEnemyDestroyed(EnemySpawner.EnemyDestroyedEvent @event)
        {
            score += @event.Score;
            EventManager.Raise(new ScoreHasChangedEvent(score));
        }
    }
}