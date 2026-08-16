using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // The bottom-center hotbar row. Drop a dragged inventory slot onto one of these to bind it.
    public sealed class HotbarHud
    {
        private readonly DragState _dragState;
        private readonly Texture2D[] _slotTextures;
        private readonly float _slotSize;
        private readonly float _slotGap;

        public HotbarHud(DragState dragState, Texture2D[] slotTextures, float slotSize, float slotGap)
        {
            _dragState = dragState;
            _slotTextures = slotTextures;
            _slotSize = slotSize;
            _slotGap = slotGap;
        }

        public void Draw(PlayerInventory playerInventory)
        {
            const int slotCount = PlayerInventory.HotbarSlotCount;
            var totalWidth = slotCount * _slotSize + (slotCount - 1) * _slotGap;
            var startX = (GuiScale.ReferenceWidth - totalWidth) / 2f;
            var y = GuiScale.ReferenceHeight - _slotSize - 24f;

            for (var hotbarIndex = 0; hotbarIndex < slotCount; hotbarIndex++)
            {
                var x = startX + hotbarIndex * (_slotSize + _slotGap);
                var rect = new Rect(x, y, _slotSize, _slotSize);
                var texture = hotbarIndex < _slotTextures.Length ? _slotTextures[hotbarIndex] : null;

                GuiDrawUtils.DrawSlot(rect, texture);
                GUI.Label(new Rect(rect.x, rect.y - 16f, rect.width, 14f), (hotbarIndex + 1).ToString(), GuiDrawUtils.CenteredLabelStyle);

                var boundItem = playerInventory != null ? playerInventory.GetHotbarItem(hotbarIndex) : null;
                if (boundItem != null && boundItem.icon != null)
                {
                    GuiDrawUtils.DrawSprite(rect, boundItem.icon);
                }

                if (_dragState.SlotIndex != -1 && Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
                {
                    var draggedItem = playerInventory.Slots[_dragState.SlotIndex].Item;
                    playerInventory.SetHotbarItem(hotbarIndex, draggedItem);
                    _dragState.SlotIndex = -1;
                }
            }
        }
    }
}
