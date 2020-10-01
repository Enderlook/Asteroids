using UnityEngine;
using UnityEngine.SceneManagement;

namespace Asteroids.UI
{
    public class SceneManagement : MonoBehaviour
    {
        public void GoToGame() => SceneManager.LoadScene(1);

        public void QuitGame() => Application.Quit();

        public void Menu()
        {
            FindObjectOfType<Pause>().TogglePause();
            SceneManager.LoadScene(0);
        }
    }
}