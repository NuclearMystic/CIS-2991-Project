using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.Levels
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TeleportPoint : MonoBehaviour
    {
        [SerializeField] private string destinationScene;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Configure(string destination)
        {
            destinationScene = destination;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (string.IsNullOrWhiteSpace(destinationScene))
                return;

            var characterSheet = other.GetComponentInParent<CIS2991Project.Player.CharacterSheet>();
            if (characterSheet == null)
                return;

            characterSheet.PreviousScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(destinationScene);
        }
    }
}
