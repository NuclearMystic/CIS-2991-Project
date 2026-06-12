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
            if (Object.FindObjectOfType<GameBootstrapper>() != null)
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

        private void EnsurePlayer()
        {
            if (Object.FindObjectOfType<PlayerHealth>() != null)
            {
                return;
            }

            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.zero;

            var rigidbody = playerObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;

            var collider = playerObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.45f;

            playerObject.AddComponent<PlayerMovement>();
            playerObject.AddComponent<PlayerHealth>();
            playerObject.AddComponent<PlayerInventory>();

            var hudObject = new GameObject("PlayerHUD");
            var hud = hudObject.AddComponent<PlayerHUD>();
            hudObject.transform.SetParent(playerObject.transform, false);
        }
    }
}
