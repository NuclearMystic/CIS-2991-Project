using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "Level1";

        private const float ButtonWidth = 220f;
        private const float ButtonHeight = 50f;
        private const float ButtonSpacing = 12f;
        private const float PanelWidth = 260f;
        private const float PanelHeight = 340f;

        private GUIStyle _titleStyle;

        private GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 36,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _titleStyle;
            }
        }

        private void OnGUI()
        {
            var panelX = (Screen.width - PanelWidth) / 2f;
            var panelY = (Screen.height - PanelHeight) / 2f;
            var buttonX = panelX + (PanelWidth - ButtonWidth) / 2f;

            GUI.Box(new Rect(panelX, panelY, PanelWidth, PanelHeight), string.Empty);
            GUI.Label(new Rect(panelX, panelY + 10f, PanelWidth, 70f), "AfterAsh", TitleStyle);

            var currentY = panelY + 90f;

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Play"))
            {
                SceneManager.LoadScene(gameplaySceneName);
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

            if (GUI.Button(new Rect(buttonX, currentY, ButtonWidth, ButtonHeight), "Exit"))
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
