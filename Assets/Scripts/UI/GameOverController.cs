using CIS2991Project.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    public class GameOverController : MonoBehaviour
    {
        private PlayerHealth _playerHealth;
        private bool _isGameOver;
        private GUIStyle _titleStyle;
        private const string MainMenuSceneName = "MainMenu";

        private GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle != null) return _titleStyle;
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 48,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.red }
                };
                return _titleStyle;
            }
        }

        private void Start()
        {
            _playerHealth = GetComponentInParent<PlayerHealth>();
            if (_playerHealth != null)
                _playerHealth.HealthChanged += OnHealthChanged;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (current > 0 || _isGameOver) return;
            _isGameOver = true;
            Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (!_isGameOver) return;

            GuiScale.Begin();

            var cx = GuiScale.ReferenceWidth / 2f;
            var cy = GuiScale.ReferenceHeight / 2f;

            GUI.Box(new Rect(cx - 150f, cy - 100f, 300f, 200f), string.Empty);
            GUI.Label(new Rect(cx - 140f, cy - 80f, 280f, 70f), "Game Over", TitleStyle);

            if (GUI.Button(new Rect(cx - 90f, cy + 10f, 180f, 45f), "Return to Main Menu"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(MainMenuSceneName);
            }
        }
    }
}
