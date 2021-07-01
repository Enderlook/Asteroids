//IA2-P3
// ^- Don't touch that comment, used by the teacher
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    [DefaultExecutionOrder((int)ExecutionOrder.O6_Score)]
    public sealed class ScoreField : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private Text score;

        [SerializeField]
        private Text kills;
#pragma warning restore CS0649

        private float currentScore;
        private int targetScore;
        private int lastScore;

        private float currentKills;
        private int targetKills;
        private int lastKills;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            Set(ref currentScore, ref lastScore, targetScore, score);
            Set(ref currentKills, ref lastKills, targetKills, kills);
        }

        public void SetTarget(int score, int kills)
        {
            targetScore = score;
            targetKills = kills;
        }

        public void SetSprite(Sprite sprite) => GetComponentInChildren<Image>().sprite = sprite;

        private void Set(ref float current, ref int lastValue, int target, Text text)
        {
            if (current != target)
            {
                current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime * (5 + target - current));
                if (current != lastValue)
                {
                    lastValue = (int)current;
                    text.text = lastValue.ToString();
                }
            }
        }
    }
}