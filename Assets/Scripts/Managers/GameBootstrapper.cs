using CIS2991Project.Player;
using CIS2991Project.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.Managers
{
    public class GameBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindAnyObjectByType<GameBootstrapper>() != null)
            {
                return;
            }

            var bootstrapperObject = new GameObject("GameBootstrapper");
            DontDestroyOnLoad(bootstrapperObject);
            bootstrapperObject.AddComponent<GameBootstrapper>().Initialize();
        }

        private void Initialize()
        {
            EnsureCamera();
            EnsurePlayer();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureCamera();
            EnsurePlayer();
        }

        private void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private const string MainMenuSceneName = "MainMenu";

        private static Sprite CreatePlaceholderSprite()
        {
            var texture = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.green;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
        }

        private void EnsurePlayer()
        {
            if (SceneManager.GetActiveScene().name == MainMenuSceneName)
            {
                return;
            }

            if (Object.FindAnyObjectByType<PlayerHealth>() != null)
            {
                return;
            }

            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = FindSpawnPoint();

            var rigidbody = playerObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;

            var collider = playerObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.45f;

            var spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreatePlaceholderSprite();
            spriteRenderer.sortingOrder = 1;

            playerObject.AddComponent<PlayerMovement>();
            playerObject.AddComponent<PlayerHealth>();
            playerObject.AddComponent<PlayerInventory>();
            playerObject.AddComponent<PlayerShoot>();

            var hudObject = new GameObject("PlayerHUD");
            hudObject.AddComponent<PlayerHUD>();
            hudObject.transform.SetParent(playerObject.transform, false);

            var pauseMenuObject = new GameObject("PauseMenu");
            pauseMenuObject.AddComponent<PauseMenuController>();
            pauseMenuObject.transform.SetParent(playerObject.transform, false);

            var gameOverObject = new GameObject("GameOver");
            gameOverObject.AddComponent<GameOverController>();
            gameOverObject.transform.SetParent(playerObject.transform, false);

            if (Camera.main != null)
            {
                Camera.main.transform.SetParent(playerObject.transform);
                Camera.main.transform.localPosition = new Vector3(0f, 0f, -10f);
            }
        }

        private static Vector3 FindSpawnPoint()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var anchorName = sceneName == "Settlement" ? "SettlementSpawn" : "RaiderBaseSpawn";
            var anchor = GameObject.Find(anchorName);
            return anchor != null ? anchor.transform.position : Vector3.zero;
        }
    }
}
