using Asteroids.Scene;

using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    [RequireComponent(typeof(Text)), DefaultExecutionOrder((int)ExecutionOrder.O6_Score)]
    public sealed class Score : MonoBehaviour
    {
        [SerializeField]
        private bool unscaledTime;

        private Text text;
        private int lastValue;
        private float current;
        private int target;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            text = GetComponent<Text>();
            EventManager.Subscribe<GameManager.ScoreHasChangedEvent>(OnScoreHasChanged);
            EventManager.Subscribe<GameManager.LevelTerminationEvent>(OnLevelTermination);
            target = GameManager.Score;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (current != target)
            {
                current = Mathf.MoveTowards(current, target, (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * (5 + target - current));
                if (current != lastValue)
                {
                    lastValue = (int)current;
                    text.text = lastValue.ToString();
                }
            }
        }

        private void OnLevelTermination(GameManager.LevelTerminationEvent @event)
        {
            if (@event.HasLost)
                unscaledTime = true;
        }

        private void OnScoreHasChanged(GameManager.ScoreHasChangedEvent @event) => target = @event.NewScore;
    }
}