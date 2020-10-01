using System;

using UnityEngine;

namespace Asteroids.UI
{
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject screen;

        [NonSerialized]
        public bool isScreenOn;

        public void TogglePauseMenu()
        {
            if (screen != null)
            {
                screen.SetActive(!screen.activeSelf);
                isScreenOn = !isScreenOn;
            }
        }
    }
}