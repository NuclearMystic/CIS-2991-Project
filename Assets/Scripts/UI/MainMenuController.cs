using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CIS2991Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "Settlement";

        // Main Menu Buttons
        private Button playButton;
        private Button loadButton;
        private Button settingsButton;
        private Button quitButton;

        // Settings Menu
        private VisualElement settingsContainer;
        private Button settingsBackButton;

        // Load Menu
        private VisualElement loadContainer;
        private Button loadBackButton;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            var root = uiDocument.rootVisualElement;

            // ===========================
            // Main Menu Buttons
            // ===========================
            playButton = root.Q<Button>("Play");
            loadButton = root.Q<Button>("Load");
            settingsButton = root.Q<Button>("Settings");
            quitButton = root.Q<Button>("Quit");

            // ===========================
            // Settings Menu
            // ===========================
            settingsContainer = root.Q<VisualElement>("SettingsContainer");
            settingsBackButton = root.Q<Button>("BackButton");

            // ===========================
            // Load Menu
            // ===========================
            loadContainer = root.Q<VisualElement>("LoadContainer");
            loadBackButton = root.Q<Button>("LoadBackButton");

            // ===========================
            // Hide Popups on Startup
            // ===========================
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.None;

            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.None;

            // ===========================
            // Register Button Events
            // ===========================
            if (playButton != null)
                playButton.clicked += PlayGame;
            else
                Debug.LogError("Play button not found.");

            if (loadButton != null)
                loadButton.clicked += OpenLoad;
            else
                Debug.LogError("Load button not found.");

            if (settingsButton != null)
                settingsButton.clicked += OpenSettings;
            else
                Debug.LogError("Settings button not found.");

            if (quitButton != null)
                quitButton.clicked += QuitGame;
            else
                Debug.LogError("Quit button not found.");

            if (settingsBackButton != null)
                settingsBackButton.clicked += CloseSettings;
            else
                Debug.LogError("Settings BackButton not found.");

            if (loadBackButton != null)
                loadBackButton.clicked += CloseLoad;
            else
                Debug.LogError("Load BackButton not found.");
        }

        // ===========================
        // PLAY
        // ===========================
        private void PlayGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        // ===========================
        // SETTINGS MENU
        // ===========================
        private void OpenSettings()
        {
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.Flex;
        }

        private void CloseSettings()
        {
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.None;
        }

        // ===========================
        // LOAD MENU
        // ===========================
        private void OpenLoad()
        {
            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.Flex;
        }

        private void CloseLoad()
        {
            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.None;
        }

        // ===========================
        // QUIT
        // ===========================
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
