using CIS2991Project.Player;
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

            PlayerHealth player;

            var existingPlayer = Object.FindAnyObjectByType<PlayerHealth>();
            if (existingPlayer != null)
            {
                // Player carried over from the previous scene via GameManager's DontDestroyOnLoad -
                // keep its inventory/stats and just drop it at the new scene's spawn point.
                var characterSheet = existingPlayer.GetComponent<CharacterSheet>();
                existingPlayer.transform.position = FindSpawnPoint(characterSheet);
                player = existingPlayer;
                Debug.Log($"[SaveDebug] EnsurePlayer: reusing existing player '{existingPlayer.gameObject.name}' (instanceID={existingPlayer.GetInstanceID()}) in scene '{SceneManager.GetActiveScene().name}'.");
            }
            else
            {
                var prefab = Resources.Load<GameObject>(PlayerPrefabResourcePath);
                if (prefab == null)
                {
                    Debug.LogError($"GameBootstrapper: no player prefab found at Resources/{PlayerPrefabResourcePath}.prefab");
                    return;
                }

                var playerObject = Instantiate(prefab, FindSpawnPoint(null), Quaternion.identity);
                playerObject.name = "Player";
                // PlayerHealth (and the rest of the gameplay components) live on the nested
                // Player_Sprite child, not the root Player object (which only carries GameManager) -
                // GetComponent alone always returned null here.
                player = playerObject.GetComponentInChildren<PlayerHealth>();
                Debug.Log($"[SaveDebug] EnsurePlayer: no existing player found - instantiated a fresh one (instanceID={player.GetInstanceID()}) in scene '{SceneManager.GetActiveScene().name}'.");
            }

            // If a Load was requested (SaveSystem.Load queued this before triggering the scene
            // switch), override whatever spawn-anchor position/state was just set above with the
            // saved one. No-ops immediately if nothing is pending.
            ApplyPendingLoad(player);
        }

        private static void ApplyPendingLoad(PlayerHealth player)
        {
            if (player == null)
            {
                return;
            }

            var inventory = player.GetComponent<PlayerInventory>();
            var characterSheet = player.GetComponent<CharacterSheet>();
            if (inventory == null || characterSheet == null)
            {
                Debug.LogWarning($"[SaveDebug] ApplyPendingLoad: bailing out, inventory={(inventory == null ? "null" : "ok")} characterSheet={(characterSheet == null ? "null" : "ok")}.");
                return;
            }

            var survivalStats = player.GetComponent<SurvivalStats>();
            var playerShoot = player.GetComponent<PlayerShoot>();
            var animationController = player.GetComponent<PlayerAnimationController>();
            var applied = SaveSystem.TryConsumePendingLoad(player, inventory, characterSheet, survivalStats, playerShoot, animationController);
            Debug.Log($"[SaveDebug] ApplyPendingLoad: TryConsumePendingLoad returned {applied} for instanceID={player.GetInstanceID()}.");
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
