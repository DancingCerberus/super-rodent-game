using UnityEngine;
using UnityEngine.SceneManagement;
using SuperRodentGame.Input;

namespace SuperRodentGame.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;

        private bool paused;

        private void Update()
        {
            if (XRIInputReader.Instance != null &&
                XRIInputReader.Instance.PausePressed())
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            paused = !paused;

            Time.timeScale = paused ? 0f : 1f;

            if (menuRoot != null)
            {
                menuRoot.SetActive(paused);
            }
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex);
        }
    }
}