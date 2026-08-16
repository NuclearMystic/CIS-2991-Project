using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Weapon / Outfit equipment boxes, bottom-left. Double-clicking either unequips it.
    public sealed class EquipmentHud
    {
        private const int WeaponBoxClickId = -100;
        private const int OutfitBoxClickId = -200;

        private readonly DoubleClickTracker _doubleClickTracker;
        private readonly float _boxSize;
        private readonly float _boxGap;

        public EquipmentHud(DoubleClickTracker doubleClickTracker, float boxSize, float boxGap)
        {
            _doubleClickTracker = doubleClickTracker;
            _boxSize = boxSize;
            _boxGap = boxGap;
        }

        public void Draw(PlayerInventory playerInventory)
        {
            if (playerInventory == null)
            {
                return;
            }

            const float margin = 16f;

            var weaponRect = new Rect(margin, GuiScale.ReferenceHeight - _boxSize - margin, _boxSize, _boxSize);
            var outfitRect = new Rect(margin + _boxSize + _boxGap, GuiScale.ReferenceHeight - _boxSize - margin, _boxSize, _boxSize);

            GUI.Label(new Rect(weaponRect.x, weaponRect.y - 18f, _boxSize, 18f), "Weapon");
            GUI.Label(new Rect(outfitRect.x, outfitRect.y - 18f, _boxSize, 18f), "Outfit");

            if (DrawEquipmentSlot(weaponRect, playerInventory.EquippedWeapon) && _doubleClickTracker.RegisterClick(WeaponBoxClickId))
            {
                playerInventory.TryUnequipWeapon();
            }

            if (DrawEquipmentSlot(outfitRect, playerInventory.EquippedArmor) && _doubleClickTracker.RegisterClick(OutfitBoxClickId))
            {
                playerInventory.TryUnequipArmor();
            }
        }

        private static bool DrawEquipmentSlot(Rect rect, global::Item item)
        {
            var clicked = GUI.Button(rect, string.Empty);
            if (item != null && item.icon != null)
            {
                GuiDrawUtils.DrawSprite(rect, item.icon);
            }
            return clicked;
        }
    }
}
