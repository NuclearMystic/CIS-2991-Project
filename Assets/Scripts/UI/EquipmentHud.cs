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

        public void Draw(PlayerInventory playerInventory, PlayerShoot playerShoot)
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

            DrawAmmoCount(weaponRect, playerShoot);

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

        // Total rounds held in the bag (PlayerShoot.ReserveAmmo) - not the magazine's current/max -
        // overlaid on the bottom edge of the weapon box. Only shown for weapons that actually use ammo
        // (melee weapons report WeaponAmmoType.None).
        private static void DrawAmmoCount(Rect weaponRect, PlayerShoot playerShoot)
        {
            if (playerShoot == null || playerShoot.CurrentAmmoType == global::WeaponAmmoType.None)
            {
                return;
            }

            const float labelHeight = 18f;
            var labelRect = new Rect(weaponRect.x, weaponRect.yMax - labelHeight, weaponRect.width, labelHeight);

            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(labelRect, Texture2D.whiteTexture);
            GUI.color = previousColor;

            GUI.Label(labelRect, playerShoot.ReserveAmmo.ToString(), GuiDrawUtils.CenteredLabelStyle);
        }
    }
}
