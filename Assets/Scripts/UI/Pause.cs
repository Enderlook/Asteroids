using UnityEngine;

namespace Asteroids.UI
{
    public class Pause : MonoBehaviour
    {
        private ScreenManager screenManager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => screenManager = GetComponent<ScreenManager>();

        public void TogglePause()
        {
            if (screenManager.isScreenOn)
                Time.timeScale = 1;
            else
                Time.timeScale = 0;
        }
    }
}
