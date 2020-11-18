using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.UI
{
    public class PauseManager : MonoBehaviour
    {
        public void TogglePause()
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                EventManager.Raise(PauseEvent.Play);
            }
            else
            {
                Time.timeScale = 0;
                EventManager.Raise(PauseEvent.Pause);
            }
        }

        public void UnPause()
        {
            Time.timeScale = 1;
            EventManager.Raise(PauseEvent.Play);
        }

        public void Pause()
        {
            Time.timeScale = 0;
            EventManager.Raise(PauseEvent.Pause);
        }

        public readonly struct PauseEvent
        {
            public readonly bool IsPaused;

            public bool IsPlaying => !IsPaused;

            public PauseEvent(bool isPaused) => IsPaused = isPaused;

            public static PauseEvent Pause => new PauseEvent(true);

            public static PauseEvent Play => new PauseEvent(false);
        }
    }
}
