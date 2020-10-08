using Asteroids.Events;
using Asteroids.Scene;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    [RequireComponent(typeof(Text))]
    public class Score : MonoBehaviour
    {
        private Text text;
        private int lastValue;
        private float current;
        private int target;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            text = GetComponent<Text>();
            EventManager.Subscribe<ScoreHasChangedEvent>(OnScoreHasChanged);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (current != target)
            {
                current = Mathf.MoveTowards(current, target, Time.deltaTime * (5 + target - current));
                if (current != lastValue)
                {
                    lastValue = (int)current;
                    text.text = lastValue.ToString();
                }
            }
        }

        private void OnScoreHasChanged(ScoreHasChangedEvent @event) => target = @event.NewScore;
    }
}