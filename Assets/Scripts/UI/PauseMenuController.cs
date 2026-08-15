using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool _isPaused;

        private const float ButtonWidth = 220f;
        private const float ButtonHeight = 45f;
        private const float ButtonSpacing = 10f;
        private const float PanelWidth = 260f;
        private const float PanelHeight = 360f;

        private GUIStyle _titleStyle;

        private GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 24,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _titleStyle;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SupplyShop.IsAnyShopOpen)
                {
                    SupplyShop.CloseActiveShop();
                    return;
                }

                SetPaused(!_isPaused);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (!_isPaused)
            {
                return;
            }

            var panelX = (Screen.width - PanelWidth) / 2f;
            var panelY = (Screen.height - PanelHeight) / 2f;
            var buttonX = panelX + (PanelWidth - ButtonWidth) / 2f;

            GUI.Box(new Rect(panelX, panelY, PanelWidth, PanelHeight), string.Empty);
            GUI.Label(new Rect(panelX, panelY + 10f, PanelWidth, 50f), "Paused", TitleStyle);

            var currentY = panelY + 70f;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Resume"))
            {
                SetPaused(false);
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Load"))
            {
                Debug.Log("Load: not yet implemented.");
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Options"))
            {
                Debug.Log("Options: not yet implemented.");
            }

            currentY += ButtonHeight + ButtonSpacing;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Main Menu"))
            {
                Time.timeScale = 1f;
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
    }
}
