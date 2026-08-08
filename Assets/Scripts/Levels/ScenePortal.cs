using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.Levels
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ScenePortal : MonoBehaviour
    {
        [SerializeField] private string destinationScene;
        [SerializeField] private string prompt = "Press E to travel";
        private bool playerInRange;

        public void Configure(string destination, string promptText)
        {
            destinationScene = destination;
            prompt = promptText;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            playerInRange = other.GetComponentInParent<CIS2991Project.Player.PlayerHealth>() != null;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<CIS2991Project.Player.PlayerHealth>() != null)
                playerInRange = false;
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(KeyCode.E) && !string.IsNullOrWhiteSpace(destinationScene))
                SceneManager.LoadScene(destinationScene);
        }

        private void OnGUI()
        {
            if (playerInRange)
                GUI.Box(new Rect(Screen.width / 2f - 120f, Screen.height - 90f, 240f, 34f), prompt);
        }
    }
}
