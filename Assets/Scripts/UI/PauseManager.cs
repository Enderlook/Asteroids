using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.UI
{
    public partial class PauseManager : MonoBehaviour
    {
        public void TogglePause()
        {
            if (Time.timeScale == 0)
            {
                Cursor.visible = true;
                Time.timeScale = 1;
                EventManager.Raise(PauseEvent.Play);
            }
            else
            {
                Cursor.visible = false;
                Time.timeScale = 0;
                EventManager.Raise(PauseEvent.Pause);
            }
        }

        public void UnPause()
        {
            Cursor.visible = false;
            Time.timeScale = 1;
            EventManager.Raise(PauseEvent.Play);
        }

        public void Pause()
        {
            Cursor.visible = true;
            Time.timeScale = 0;
            EventManager.Raise(PauseEvent.Pause);
        }
    }
}
