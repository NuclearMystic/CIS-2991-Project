using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.World
{
    [RequireComponent(typeof(Collider2D))]
    public class NpcDialogue : MonoBehaviour
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField, TextArea] private string dialogueLine = "Hello.";

        private bool playerInRange;
        private bool dialogueOpen;

        public void Configure(string displayName, string line)
        {
            npcName = displayName;
            dialogueLine = line;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
            {
                playerInRange = false;
                dialogueOpen = false;
            }
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                dialogueOpen = !dialogueOpen;
            }

            if (dialogueOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                dialogueOpen = false;
            }
        }

        private void OnGUI()
        {
            if (!playerInRange)
            {
                return;
            }

            GUI.Box(new Rect(16f, 150f, 280f, 70f), string.Empty);
            GUI.Label(new Rect(28f, 162f, 240f, 20f), $"Press E to talk to {npcName}");

            if (!dialogueOpen)
            {
                return;
            }

            GUI.Box(new Rect(16f, 232f, 360f, 110f), npcName);
            GUI.Label(new Rect(28f, 260f, 320f, 60f), dialogueLine);
            GUI.Label(new Rect(28f, 316f, 260f, 20f), "Press Esc to close");
        }
    }
}
