using Asteroids.Events;

using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Asteroids.UI
{
    public class Menu : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private bool isMainMenu;

        [SerializeField, Tooltip("Key pressed to open and close menu.")]
        private KeyCode menu;

        [SerializeField, Tooltip("Panels to toggle.")]
        private GameObject[] panels;
#pragma warning restore CS0649

        private PauseManager pause;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            pause = FindObjectOfType<PauseManager>();
            EventManager.Subscribe<PauseEvent>(OnPause);
        }

        private void OnPause(PauseEvent @event)
        {
            if (@event.IsPaused)
                panels[0].SetActive(true);
            else
                panels[0].SetActive(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (Input.GetKeyDown(menu))
            {
                if (isMainMenu)
                {
                    for (int i = 0; i < panels.Length; i++)
                        panels[i].SetActive(false);
                }
                else if (panels[0].activeSelf)
                {
                    for (int i = panels.Length - 1; i >= 0; i--)
                    {
                        GameObject subPanel = panels[i];
                        if (subPanel.activeSelf)
                        {
                            subPanel.SetActive(false);
                            break;
                        }
                    }
                    pause.UnPause();
                }
                else
                    pause.Pause();
            }
        }

        public void GoToGame()
        {
            if (pause != null)
                pause.UnPause();
            SceneManager.LoadScene(1);
        }

        public void QuitGame() => Application.Quit();

        public void GotToMenu()
        {
            pause.UnPause();
            SceneManager.LoadScene(0);
        }
    }
}