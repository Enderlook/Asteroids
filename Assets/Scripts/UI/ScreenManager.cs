using Asteroids.Events;

using UnityEngine;

namespace Asteroids.UI
{
    public class ScreenManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private GameObject screen;
#pragma warning restore CS0649

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => EventManager.Subscribe<PauseEvent>(OnPause);

        public void OnPause(PauseEvent @event)
        {
            if (screen != null)
                screen.SetActive(@event.IsPaused);
        }
    }
}