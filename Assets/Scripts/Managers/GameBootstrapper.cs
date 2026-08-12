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
            EnsurePlayer();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsurePlayer();
        }

        private const string MainMenuSceneName = "MainMenu";
        private const string PlayerPrefabResourcePath = "Prefabs/Characters/Player/Player";

        private void EnsurePlayer()
        {
            if (SceneManager.GetActiveScene().name == MainMenuSceneName)
            {
                return;
            }

            var existingPlayer = Object.FindAnyObjectByType<PlayerHealth>();
            if (existingPlayer != null)
            {
                // Player carried over from the previous scene via CharacterSheet's DontDestroyOnLoad -
                // keep its inventory/stats and just drop it at the new scene's spawn point.
                var characterSheet = existingPlayer.GetComponent<CharacterSheet>();
                existingPlayer.transform.position = FindSpawnPoint(characterSheet);
                return;
            }

            var prefab = Resources.Load<GameObject>(PlayerPrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"GameBootstrapper: no player prefab found at Resources/{PlayerPrefabResourcePath}.prefab");
                return;
            }

            var playerObject = Instantiate(prefab, FindSpawnPoint(null), Quaternion.identity);
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
        }

        private static Vector3 FindSpawnPoint(CharacterSheet characterSheet)
        {
            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "Overworld")
            {
                var previousScene = characterSheet != null ? characterSheet.PreviousScene : null;
                var returnAnchor = string.IsNullOrEmpty(previousScene)
                    ? null
                    : GameObject.Find("Overworld_" + previousScene);
                var anchor = returnAnchor != null ? returnAnchor : GameObject.Find("OverworldSpawn");
                return anchor != null ? anchor.transform.position : Vector3.zero;
            }

            var sceneAnchor = GameObject.Find(sceneName + "Spawn");
            return sceneAnchor != null ? sceneAnchor.transform.position : Vector3.zero;
        }
    }
}
