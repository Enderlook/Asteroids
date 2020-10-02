using Asteroids.Events;

using UnityEngine;

namespace Asteroids.UI
{
    public class Pause : MonoBehaviour
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
    }
}
