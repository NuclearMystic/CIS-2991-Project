using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CIS2991Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;

        [SerializeField]
        private string gameplaySceneName = "PrototypeLevel";

        private Button playButton;
        private Button loadButton;
        private Button settingsButton;
        private Button quitButton;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            var root = uiDocument.rootVisualElement;

            playButton = root.Q<Button>("Play");
            loadButton = root.Q<Button>("Load");
            settingsButton = root.Q<Button>("Settings");
            quitButton = root.Q<Button>("Quit");

            playButton.clicked += PlayGame;
            loadButton.clicked += OpenLoad;
            settingsButton.clicked += OpenSettings;
            quitButton.clicked += QuitGame;
        }

        private void PlayGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void OpenLoad()
        {
            Debug.Log("Open Load Menu");
        }

        private void OpenSettings()
        {
            Debug.Log("Open Settings");
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}