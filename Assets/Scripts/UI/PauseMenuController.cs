using System;
using CIS2991Project.Jobs;
using CIS2991Project.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool _isPaused;
        private bool _showingOptions;

        private const float ButtonWidth = 220f;
        private const float ButtonHeight = 45f;
        private const float ButtonSpacing = 10f;
        private const float PanelWidth = 260f;
        private const float PanelHeight = 415f;
        private const float OptionsPanelHeight = 330f;
        private const float SliderRowHeight = 50f;

        private GUIStyle _titleStyle;

        private GUIStyle TitleStyle => GuiDrawUtils.GetOrCreate(ref _titleStyle, () => new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        });

        // True while any other window (the shared Save/Load panel) is on top of this one - lets
        // Update/OnGUI both agree on "only one window open at a time" without duplicating the check.
        private static bool AnotherWindowIsOpen =>
            SaveLoadPanelController.Instance != null && SaveLoadPanelController.Instance.IsOpen;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            // Escape always backs out one level - close whichever sub-window is on top first,
            // rather than unpausing underneath it.
            if (AnotherWindowIsOpen)
            {
                SaveLoadPanelController.Instance.Close();
                return;
            }

            if (_showingOptions)
            {
                _showingOptions = false;
                return;
            }

            SetPaused(!_isPaused);
        }

        private void OnDestroy()
        {
            if (_isPaused)
            {
                PauseGate.Release(this);
            }
        }

        private void SetPaused(bool paused)
        {
            if (paused == _isPaused)
            {
                return;
            }

            _isPaused = paused;
            if (!paused)
            {
                // Land back on the main pause panel next time it opens, rather than resuming into
                // whatever sub-window happened to be showing.
                _showingOptions = false;
            }

            if (paused)
            {
                PauseGate.Request(this);
            }
            else
            {
                PauseGate.Release(this);
            }
        }

        private void OnGUI()
        {
            if (!_isPaused || SaveSystem.IsCapturingScreenshot)
            {
                return;
            }

            // The shared Save/Load panel is a separate always-on-top window - hide this one entirely
            // while it's up instead of letting both draw at once.
            if (AnotherWindowIsOpen)
            {
                return;
            }

            GuiScale.Begin();

            if (_showingOptions)
            {
                DrawOptionsPanel();
                return;
            }

            var panelX = (GuiScale.ReferenceWidth - PanelWidth) / 2f;
            var panelY = (GuiScale.ReferenceHeight - PanelHeight) / 2f;
            var buttonX = panelX + (PanelWidth - ButtonWidth) / 2f;

            GUI.Box(new Rect(panelX, panelY, PanelWidth, PanelHeight), string.Empty);
            GUI.Label(new Rect(panelX, panelY + 10f, PanelWidth, 50f), "Paused", TitleStyle);

            var currentY = panelY + 70f;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Resume"))
            {
                SetPaused(false);
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Save"))
            {
                if (SaveLoadPanelController.Instance != null)
                    SaveLoadPanelController.Instance.Open(SaveLoadPanelController.PanelMode.Save);
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Load"))
            {
                if (SaveLoadPanelController.Instance != null)
                    SaveLoadPanelController.Instance.Open(SaveLoadPanelController.PanelMode.Load);
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Options"))
            {
                _showingOptions = true;
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Main Menu"))
            {
                // Leaving gameplay entirely - deactivate the whole persistent Player rig (see
                // GameOverController for the same pattern and why Destroy() isn't used here) so the
                // next game starts completely fresh instead of carrying over stale state.
                transform.root.gameObject.SetActive(false);
                if (SaveLoadPanelController.Instance != null)
                    SaveLoadPanelController.Instance.Close();
                PauseGate.ResetAll();
                JobManager.ResetAll();
                SceneManager.LoadScene(mainMenuSceneName);
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Quit"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void DrawOptionsPanel()
        {
            var panelX = (GuiScale.ReferenceWidth - PanelWidth) / 2f;
            var panelY = (GuiScale.ReferenceHeight - OptionsPanelHeight) / 2f;
            var rowX = panelX + 15f;
            var rowWidth = PanelWidth - 30f;

            GUI.Box(new Rect(panelX, panelY, PanelWidth, OptionsPanelHeight), string.Empty);
            GUI.Label(new Rect(panelX, panelY + 10f, PanelWidth, 40f), "Options", TitleStyle);

            var currentY = panelY + 60f;

            currentY = DrawVolumeSlider(rowX, currentY, rowWidth, "Master", AudioManager.MasterVolume, v => AudioManager.MasterVolume = v);
            currentY = DrawVolumeSlider(rowX, currentY, rowWidth, "Music", AudioManager.MusicVolume, v => AudioManager.MusicVolume = v);
            currentY = DrawVolumeSlider(rowX, currentY, rowWidth, "Ambience", AudioManager.AmbienceVolume, v => AudioManager.AmbienceVolume = v);
            currentY = DrawVolumeSlider(rowX, currentY, rowWidth, "SFX", AudioManager.SfxVolume, v => AudioManager.SfxVolume = v);

            currentY += ButtonSpacing;

            if (GUI.Button(new Rect(rowX, currentY, rowWidth, ButtonHeight), "Back"))
            {
                _showingOptions = false;
            }
        }

        private static float DrawVolumeSlider(float x, float y, float width, string label, float value, Action<float> onChanged)
        {
            GUI.Label(new Rect(x, y, width, 20f), label);

            var newValue = GUI.HorizontalSlider(new Rect(x, y + 20f, width, 20f), value, 0f, 1f);
            if (!Mathf.Approximately(newValue, value))
            {
                onChanged(newValue);
            }

            return y + SliderRowHeight;
        }
    }
}
