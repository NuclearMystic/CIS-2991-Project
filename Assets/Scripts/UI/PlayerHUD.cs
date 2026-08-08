using CIS2991Project.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using CIS2991Project.Enemies;

namespace CIS2991Project.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        private const int InventoryColumns = 5;

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;

        private bool inventoryVisible;
        private int selectedInventorySlot = -1;

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

        private void Update()
        {
            if (Input.GetKeyDown(inventoryToggleKey))
            {
                inventoryVisible = !inventoryVisible;
            }
        }

        private void OnGUI()
        {
            DrawHealthHud();
            DrawInventoryToggleButton();

            if (inventoryVisible)
            {
                DrawInventoryHud();
            }
        }

        private void DrawHealthHud()
        {
            if (playerHealth == null)
            {
                return;
            }

            GUI.Box(new Rect(16f, 16f, 180f, 50f), string.Empty);
            GUI.Label(new Rect(28f, 28f, 160f, 24f), $"HP: {playerHealth.CurrentHealth} / {playerHealth.MaxHealth}");
            GUI.Label(new Rect(16f, 72f, 420f, 22f), $"{SceneManager.GetActiveScene().name}  |  Move: WASD/Arrows  Shoot: Space  Inventory: I");
            if (SceneManager.GetActiveScene().name == "RaiderBase")
                GUI.Label(new Rect(16f, 94f, 240f, 22f), $"Zombies remaining: {DemoEnemy.ActiveZombieCount}");
        }

        private void DrawInventoryToggleButton()
        {
            if (GUI.Button(new Rect(208f, 16f, 90f, 50f), "Bag"))
            {
                inventoryVisible = !inventoryVisible;
            }
        }

        private void DrawInventoryHud()
        {
            if (playerInventory == null)
            {
                return;
            }

            const float startX = 16f;
            const float startY = 82f;
            const float width = 430f;
            const float height = 360f;
            const float slotWidth = 78f;
            const float slotHeight = 36f;
            const float slotGap = 4f;
            const float gridStartX = startX + 12f;
            const float gridStartY = startY + 34f;

            GUI.Box(new Rect(startX, startY, width, height), string.Empty);

            var slots = playerInventory.Slots;
            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var row = slotIndex / InventoryColumns;
                var column = slotIndex % InventoryColumns;
                var slotX = gridStartX + column * (slotWidth + slotGap);
                var slotY = gridStartY + row * (slotHeight + slotGap);
                var slotLabel = BuildSlotLabel(slots[slotIndex]);

                if (GUI.Button(new Rect(slotX, slotY, slotWidth, slotHeight), slotLabel))
                {
                    selectedInventorySlot = slotIndex;
                }
            }

            DrawSelectedItemPanel(startX + 12f, startY + 242f, width - 24f);

            if (!string.IsNullOrWhiteSpace(playerInventory.LastMessage))
            {
                GUI.Label(new Rect(startX + 12f, startY + height - 26f, width - 24f, 20f), playerInventory.LastMessage);
            }
        }

        private void DrawSelectedItemPanel(float startX, float startY, float width)
        {
            if (!playerInventory.IsValidSlot(selectedInventorySlot))
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Select an item.");
                return;
            }

            var selectedSlot = playerInventory.Slots[selectedInventorySlot];
            if (selectedSlot.IsEmpty)
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Empty");
                return;
            }

            var selectedItem = selectedSlot.Item;
            var itemName = GetItemName(selectedItem);
            var countText = selectedSlot.Amount > 1 ? $" x{selectedSlot.Amount}" : string.Empty;

            GUI.Label(new Rect(startX, startY, width, 20f), $"{itemName}{countText} ({selectedItem.itemType})");
            GUI.Label(new Rect(startX, startY + 20f, width, 20f), selectedItem.description);

            var buttonX = startX;
            var buttonY = startY + 44f;
            if (playerInventory.CanUseAt(selectedInventorySlot))
            {
                if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Use"))
                {
                    playerInventory.TryUseAt(selectedInventorySlot, playerHealth);
                }

                buttonX += 70f;
            }

            if (playerInventory.CanEquipAt(selectedInventorySlot))
            {
                if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Equip"))
                {
                    playerInventory.TryEquipAt(selectedInventorySlot);
                }

                buttonX += 70f;
            }

            if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Drop"))
            {
                if (playerInventory.TryDropAt(selectedInventorySlot))
                {
                    var slotStillValid = playerInventory.IsValidSlot(selectedInventorySlot);
                    if (!slotStillValid || playerInventory.Slots[selectedInventorySlot].IsEmpty)
                    {
                        selectedInventorySlot = -1;
                    }
                }
            }
        }

        private static string BuildSlotLabel(PlayerInventory.InventorySlot slot)
        {
            if (slot.IsEmpty)
            {
                return string.Empty;
            }

            var amountText = slot.Amount > 1 ? $" x{slot.Amount}" : string.Empty;
            return $"{GetItemName(slot.Item)}{amountText}";
        }

        private static string GetItemName(global::Item item)
        {
            if (item == null)
            {
                return "Empty";
            }

            return string.IsNullOrWhiteSpace(item.displayName) ? item.name : item.displayName;
        }
    }
}
