using CIS2991Project.Items;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // The player's inventory grid (left side) and, when a chest is open, its grid (right side) - plus
    // the selected-item detail panel and the item icon that follows the mouse while dragging.
    public sealed class InventoryPanel
    {
        private const int Columns = 5;

        private readonly DragState _dragState;
        private readonly DoubleClickTracker _doubleClickTracker;
        private readonly float _panelWidth;
        private readonly float _panelHeight;
        private readonly float _slotWidth;
        private readonly float _slotHeight;
        private readonly float _slotGap;
        private readonly float _gridTopPadding;
        private readonly Texture2D _borderTexture;
        private readonly Texture2D _slotBackgroundTexture;

        private int _selectedInventorySlot = -1;

        public InventoryPanel(
            DragState dragState, DoubleClickTracker doubleClickTracker,
            float panelWidth, float panelHeight, float slotWidth, float slotHeight, float slotGap, float gridTopPadding,
            Texture2D borderTexture, Texture2D slotBackgroundTexture)
        {
            _dragState = dragState;
            _doubleClickTracker = doubleClickTracker;
            _panelWidth = panelWidth;
            _panelHeight = panelHeight;
            _slotWidth = slotWidth;
            _slotHeight = slotHeight;
            _slotGap = slotGap;
            _gridTopPadding = gridTopPadding;
            _borderTexture = borderTexture;
            _slotBackgroundTexture = slotBackgroundTexture;
        }

        public void DrawDraggedPreview(PlayerInventory playerInventory)
        {
            if (_dragState.SlotIndex < 0 || playerInventory == null || !playerInventory.IsValidSlot(_dragState.SlotIndex))
            {
                return;
            }

            var slot = playerInventory.Slots[_dragState.SlotIndex];
            if (slot.IsEmpty || slot.Item.icon == null)
            {
                return;
            }

            const float previewSize = 32f;
            var mousePosition = Event.current.mousePosition;
            var previewRect = new Rect(mousePosition.x - previewSize / 2f, mousePosition.y - previewSize / 2f, previewSize, previewSize);
            GuiDrawUtils.DrawSprite(previewRect, slot.Item.icon);
        }

        public void DrawPlayerInventory(PlayerInventory playerInventory, PlayerHealth playerHealth, float startY)
        {
            if (playerInventory == null)
            {
                return;
            }

            const float startX = 16f;
            var width = _panelWidth;
            var height = _panelHeight;
            var gridStartX = startX + 12f;
            var gridStartY = startY + _gridTopPadding;

            GuiDrawUtils.DrawSlot(new Rect(startX, startY, width, height), _borderTexture);

            var slots = playerInventory.Slots;
            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var row = slotIndex / Columns;
                var column = slotIndex % Columns;
                var slotX = gridStartX + column * (_slotWidth + _slotGap);
                var slotY = gridStartY + row * (_slotHeight + _slotGap);
                var slotRect = new Rect(slotX, slotY, _slotWidth, _slotHeight);
                var slot = slots[slotIndex];

                if (Event.current.type == EventType.MouseDown && slotRect.Contains(Event.current.mousePosition) &&
                    !slot.IsEmpty && IsHotbarEligible(slot.Item))
                {
                    _dragState.SlotIndex = slotIndex;
                }

                if (DrawSlotBox(slotRect, slot))
                {
                    if (!slot.IsEmpty && PlayerInventory.IsEquipment(slot.Item))
                    {
                        if (_doubleClickTracker.RegisterClick(slotIndex))
                        {
                            playerInventory.TryEquipAt(slotIndex);
                        }
                    }
                    else
                    {
                        _selectedInventorySlot = slotIndex;
                    }
                }
            }

            var rowCount = Mathf.CeilToInt(slots.Count / (float)Columns);
            var gridBottomY = gridStartY + rowCount * (_slotHeight + _slotGap);
            DrawSelectedItemPanel(playerInventory, playerHealth, startX + 12f, gridBottomY + 8f, width - 24f);

            if (!string.IsNullOrWhiteSpace(playerInventory.LastMessage))
            {
                GUI.Label(new Rect(startX + 12f, startY + height - 26f, width - 24f, 20f), playerInventory.LastMessage);
            }
        }

        // Mirrors DrawPlayerInventory's grid rendering, reading from the currently open ChestInventory
        // instead of the player's own inventory. Click-to-take only (no drag-and-drop) - clicking a
        // slot hands its whole stack to the player and clears it from the chest.
        public void DrawChestInventory(PlayerInventory playerInventory, ChestInventory chest, float startY)
        {
            if (chest == null || playerInventory == null)
            {
                return;
            }

            const float margin = 16f;
            var width = _panelWidth;
            var startX = GuiScale.ReferenceWidth - width - margin;
            var gridStartX = startX + 12f;
            var gridStartY = startY + _gridTopPadding;

            var slots = chest.Slots;
            var rowCount = Mathf.Max(1, Mathf.CeilToInt(slots.Count / (float)Columns));
            var height = _gridTopPadding + rowCount * (_slotHeight + _slotGap) + 16f;

            GuiDrawUtils.DrawSlot(new Rect(startX, startY, width, height), _borderTexture);
            GUI.Label(new Rect(startX + 12f, startY + 4f, width - 24f, 20f), "Chest (click to take)");

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (slot.IsEmpty)
                {
                    continue;
                }

                var row = slotIndex / Columns;
                var column = slotIndex % Columns;
                var slotX = gridStartX + column * (_slotWidth + _slotGap);
                var slotY = gridStartY + row * (_slotHeight + _slotGap);
                var slotRect = new Rect(slotX, slotY, _slotWidth, _slotHeight);

                if (DrawSlotBox(slotRect, slot) && playerInventory.TryAdd(slot.Item, slot.Amount))
                {
                    chest.ClearSlot(slotIndex);
                }
            }
        }

        // The texture-or-label slot box shared by both grids above - returns whether it was clicked.
        private bool DrawSlotBox(Rect slotRect, PlayerInventory.InventorySlot slot)
        {
            var slotLabel = BuildSlotLabel(slot);

            if (_slotBackgroundTexture != null)
            {
                var clicked = GUI.Button(slotRect, string.Empty);
                GUI.DrawTexture(slotRect, _slotBackgroundTexture, ScaleMode.ScaleToFit);
                if (!string.IsNullOrEmpty(slotLabel))
                {
                    GUI.Label(slotRect, slotLabel, GuiDrawUtils.CenteredLabelStyle);
                }
                return clicked;
            }

            return GUI.Button(slotRect, slotLabel);
        }

        private void DrawSelectedItemPanel(PlayerInventory playerInventory, PlayerHealth playerHealth, float startX, float startY, float width)
        {
            if (!playerInventory.IsValidSlot(_selectedInventorySlot))
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Select an item.");
                return;
            }

            var selectedSlot = playerInventory.Slots[_selectedInventorySlot];
            if (selectedSlot.IsEmpty)
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Empty");
                return;
            }

            var selectedItem = selectedSlot.Item;
            var itemName = GuiDrawUtils.GetItemName(selectedItem);
            var countText = selectedSlot.Amount > 1 ? $" x{selectedSlot.Amount}" : string.Empty;

            GUI.Label(new Rect(startX, startY, width, 20f), $"{itemName}{countText} ({selectedItem.itemType})");
            GUI.Label(new Rect(startX, startY + 20f, width, 20f), selectedItem.description);

            var buttonX = startX;
            var buttonY = startY + 44f;
            if (playerInventory.CanUseAt(_selectedInventorySlot))
            {
                if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Use"))
                {
                    playerInventory.TryUseAt(_selectedInventorySlot, playerHealth);
                }

                buttonX += 70f;
            }

            if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Drop"))
            {
                if (playerInventory.TryDropAt(_selectedInventorySlot))
                {
                    var slotStillValid = playerInventory.IsValidSlot(_selectedInventorySlot);
                    if (!slotStillValid || playerInventory.Slots[_selectedInventorySlot].IsEmpty)
                    {
                        _selectedInventorySlot = -1;
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
            return $"{GuiDrawUtils.GetItemName(slot.Item)}{amountText}";
        }

        private static bool IsHotbarEligible(global::Item item)
        {
            return item != null &&
                   (PlayerInventory.IsEquipment(item) || item.itemType == global::ItemType.Consumable);
        }
    }
}
