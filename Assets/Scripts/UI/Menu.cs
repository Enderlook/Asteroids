using Asteroids.Scene;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [SerializeField, Tooltip("This button is disable if we don't have a save file.")]
        private Button disableIfNotSave;
#pragma warning restore CS0649

        private PauseManager pause;

        private bool isLock;

        private const int MAIN_MENU_SCENE = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            pause = FindObjectOfType<PauseManager>();
            EventManager.Subscribe<PauseManager.PauseEvent>(OnPause);
            EventManager.Subscribe<GameManager.LevelTerminationEvent>(OnLevelTermination);
            if (disableIfNotSave != null)
                disableIfNotSave.interactable = GameSaver.HasSaveFile();
        }

        private void OnLevelTermination(GameManager.LevelTerminationEvent @event)
        {
            if (@event.HasLost)
            {
                isLock = true;
                for (int i = 0; i < panels.Length; i++)
                    panels[i].SetActive(false);
            }
        }

        private void OnPause(PauseManager.PauseEvent @event)
        {
            if (@event.IsPaused)
                panels[0].SetActive(true);
            else
                panels[0].SetActive(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (!isLock && Input.GetKeyDown(menu))
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

        public void GoToGame(bool @continue)
        {
            GameSaver.requestLoad = @continue;
            if (pause != null)
                pause.UnPause();
            Load(1);
        }

        public void QuitGame() => Application.Quit();

        public void GotToMenu() => Load(MAIN_MENU_SCENE);

        private void Load(int scene = 0) => SceneLoading(SceneManager.LoadSceneAsync(scene));

        private AsyncOperation SceneLoading(AsyncOperation asyncOperation)
        {
            asyncOperation.completed += _ => FindObjectOfType<PauseManager>().UnPause();
            return asyncOperation;
        }
    }
}