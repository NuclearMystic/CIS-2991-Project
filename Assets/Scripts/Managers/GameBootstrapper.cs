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
        private const string PlayerPrefabResourcePath = "Prefabs/Characters/Player/Player";

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

            var prefab = Resources.Load<GameObject>(PlayerPrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"GameBootstrapper: no player prefab found at Resources/{PlayerPrefabResourcePath}.prefab");
                return;
            }

            var playerObject = Instantiate(prefab, FindSpawnPoint(), Quaternion.identity);
            playerObject.name = "Player";

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
