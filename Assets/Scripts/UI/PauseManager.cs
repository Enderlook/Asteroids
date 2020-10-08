using Asteroids.Events;

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
    }
}
