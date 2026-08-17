using CIS2991Project.Jobs;
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

        private GUIStyle TitleStyle => GuiDrawUtils.GetOrCreate(ref _titleStyle, () => new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.red }
        });

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

            if (_isGameOver)
                PauseGate.Release(this);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (current > 0 || _isGameOver) return;
            _isGameOver = true;
            PauseGate.Request(this);
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
                // The Player (and this controller with it) survives scene loads via GameManager's
                // DontDestroyOnLoad, since it's meant to persist between gameplay scenes mid-run.
                // Returning to the main menu abandons the run entirely. Deactivating the whole rig
                // (rather than Destroy-ing it) stops its HUD/Update/OnGUI dead in its tracks - so it
                // can't leak stale state (HUD, facing direction, ...) into the next game - without
                // tearing down a live DontDestroyOnLoad hierarchy's components mid-frame, which
                // hung the Editor when tried. GameBootstrapper instantiates a fresh Player prefab
                // next time a gameplay scene loads, since FindAnyObjectByType skips inactive objects.
                transform.root.gameObject.SetActive(false);
                PauseGate.ResetAll();
                JobManager.ResetAll();
                SceneManager.LoadScene(MainMenuSceneName);
            }
        }
    }
}
