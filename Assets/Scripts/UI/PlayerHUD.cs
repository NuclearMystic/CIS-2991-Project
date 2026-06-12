using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInventory playerInventory;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
            }

            if (playerInventory == null)
            {
                playerInventory = GetComponentInParent<PlayerInventory>();
            }

            if (playerInventory == null)
            {
                playerInventory = Object.FindAnyObjectByType<PlayerInventory>();
            }

            EnsureHud();
        }

        private void EnsureHud()
        {
            // IMGUI HUD needs no scene setup; OnGUI draws it every frame.
        }

        private void OnGUI()
        {
            DrawHealthHud();
            DrawInventoryHud();
        }

        private void DrawHealthHud()
        {
            if (playerHealth == null)
            {
                return;
            }

            GUI.Box(new Rect(16f, 16f, 180f, 50f), string.Empty);
            GUI.Label(new Rect(28f, 28f, 160f, 24f), $"HP: {playerHealth.CurrentHealth} / {playerHealth.MaxHealth}");
        }

        private void DrawInventoryHud()
        {
            if (playerInventory == null)
            {
                return;
            }

            const float startX = 16f;
            const float startY = 82f;
            const float width = 240f;
            const float rowHeight = 28f;

            GUI.Box(new Rect(startX, startY, width, 120f), "Inventory");

            var stacks = playerInventory.Consumables;
            if (stacks.Count == 0)
            {
                GUI.Label(new Rect(startX + 12f, startY + 34f, width - 24f, 20f), "No consumables yet.");
                return;
            }

            var currentY = startY + 32f;
            for (var index = 0; index < stacks.Count && index < 3; index++)
            {
                var stack = stacks[index];
                var label = $"{stack.item.DisplayName} x{stack.amount}";
                GUI.Label(new Rect(startX + 12f, currentY, 150f, 20f), label);

                if (GUI.Button(new Rect(startX + 160f, currentY - 2f, 68f, 22f), "Use"))
                {
                    playerInventory.TryConsume(stack.item, playerHealth);
                }

                currentY += rowHeight;
            }
        }
    }
}
