using CIS2991Project.Managers;
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
        private Slider masterSlider;
        private Slider musicSlider;
        private Slider ambianceSlider;
        private Slider sfxSlider;

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
            masterSlider = root.Q<Slider>("MasterSlider");
            musicSlider = root.Q<Slider>("MusicSlider");
            ambianceSlider = root.Q<Slider>("AmbianceSlider");
            sfxSlider = root.Q<Slider>("SFXSlider");

            // ===========================
            // Hide Popups on Startup
            // ===========================
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.None;

            InitializeVolumeSlider(masterSlider, () => AudioManager.MasterVolume, v => AudioManager.MasterVolume = v);
            InitializeVolumeSlider(musicSlider, () => AudioManager.MusicVolume, v => AudioManager.MusicVolume = v);
            InitializeVolumeSlider(ambianceSlider, () => AudioManager.AmbienceVolume, v => AudioManager.AmbienceVolume = v);
            InitializeVolumeSlider(sfxSlider, () => AudioManager.SfxVolume, v => AudioManager.SfxVolume = v);

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

        // Sliders are authored 0-100 (see MainMenu.uxml), AudioManager works in 0-1 - converts both
        // ways and keeps the slider's on-screen position in sync with whatever was loaded from
        // PlayerPrefs last session.
        private static void InitializeVolumeSlider(Slider slider, System.Func<float> getVolume, System.Action<float> setVolume)
        {
            if (slider == null)
                return;

            slider.SetValueWithoutNotify(getVolume() * 100f);
            slider.RegisterValueChangedCallback(evt => setVolume(evt.newValue / 100f));
        }

        // ===========================
        // LOAD MENU - shared with the in-game pause menu's Save/Load, see SaveLoadPanelController.
        // ===========================
        private void OpenLoad()
        {
            if (SaveLoadPanelController.Instance != null)
                SaveLoadPanelController.Instance.Open(SaveLoadPanelController.PanelMode.Load);
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
