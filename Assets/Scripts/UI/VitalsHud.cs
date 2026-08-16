using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Hearts, the ammo row, and the floating reload bar - the "how much HP/ammo do I have" readouts.
    public sealed class VitalsHud
    {
        private readonly Texture2D _fullHeartTexture;
        private readonly Texture2D _halfHeartTexture;
        private readonly Texture2D _emptyHeartTexture;
        private readonly int _fallbackHeartCount;
        private readonly int _heartsPerRow;
        private readonly float _heartSize;
        private readonly float _heartSpacing;

        private readonly Texture2D _pistolAmmoFullTexture;
        private readonly Texture2D _pistolAmmoEmptyTexture;
        private readonly Texture2D _shotgunAmmoFullTexture;
        private readonly Texture2D _shotgunAmmoEmptyTexture;
        private readonly Texture2D _rifleAmmoFullTexture;
        private readonly Texture2D _rifleAmmoEmptyTexture;
        private readonly int _ammoPerRow;
        private readonly float _ammoIconSize;
        private readonly float _ammoIconSpacing;

        private readonly Texture2D _reloadBarBackgroundTexture;
        private readonly Texture2D _reloadBarFillTexture;
        private readonly float _reloadBarWidth;
        private readonly float _reloadBarHeight;
        private readonly float _reloadBarWorldOffset;

        public VitalsHud(
            Texture2D fullHeartTexture, Texture2D halfHeartTexture, Texture2D emptyHeartTexture,
            int fallbackHeartCount, int heartsPerRow, float heartSize, float heartSpacing,
            Texture2D pistolAmmoFullTexture, Texture2D pistolAmmoEmptyTexture,
            Texture2D shotgunAmmoFullTexture, Texture2D shotgunAmmoEmptyTexture,
            Texture2D rifleAmmoFullTexture, Texture2D rifleAmmoEmptyTexture,
            int ammoPerRow, float ammoIconSize, float ammoIconSpacing,
            Texture2D reloadBarBackgroundTexture, Texture2D reloadBarFillTexture,
            float reloadBarWidth, float reloadBarHeight, float reloadBarWorldOffset)
        {
            _fullHeartTexture = fullHeartTexture;
            _halfHeartTexture = halfHeartTexture;
            _emptyHeartTexture = emptyHeartTexture;
            _fallbackHeartCount = fallbackHeartCount;
            _heartsPerRow = heartsPerRow;
            _heartSize = heartSize;
            _heartSpacing = heartSpacing;

            _pistolAmmoFullTexture = pistolAmmoFullTexture;
            _pistolAmmoEmptyTexture = pistolAmmoEmptyTexture;
            _shotgunAmmoFullTexture = shotgunAmmoFullTexture;
            _shotgunAmmoEmptyTexture = shotgunAmmoEmptyTexture;
            _rifleAmmoFullTexture = rifleAmmoFullTexture;
            _rifleAmmoEmptyTexture = rifleAmmoEmptyTexture;
            _ammoPerRow = ammoPerRow;
            _ammoIconSize = ammoIconSize;
            _ammoIconSpacing = ammoIconSpacing;

            _reloadBarBackgroundTexture = reloadBarBackgroundTexture;
            _reloadBarFillTexture = reloadBarFillTexture;
            _reloadBarWidth = reloadBarWidth;
            _reloadBarHeight = reloadBarHeight;
            _reloadBarWorldOffset = reloadBarWorldOffset;
        }

        public int GetHeartCount(CharacterSheet characterSheet) =>
            characterSheet != null ? characterSheet.GetHeartCount() : _fallbackHeartCount;

        public float GetHeartsRowWidth(CharacterSheet characterSheet) =>
            Mathf.Min(GetHeartCount(characterSheet), _heartsPerRow) * (_heartSize + _heartSpacing);

        public float DrawHearts(PlayerHealth playerHealth, CharacterSheet characterSheet)
        {
            const float startX = 16f;
            const float startY = 16f;

            if (playerHealth == null)
            {
                return startY;
            }

            var heartCountValue = GetHeartCount(characterSheet);
            var rows = Mathf.CeilToInt(heartCountValue / (float)_heartsPerRow);
            var rowHeight = _heartSize + _heartSpacing;

            var filledHalfUnits = playerHealth.MaxHealth > 0
                ? Mathf.RoundToInt(playerHealth.CurrentHealth / (float)playerHealth.MaxHealth * heartCountValue * 2)
                : 0;

            for (var heartIndex = 0; heartIndex < heartCountValue; heartIndex++)
            {
                var row = heartIndex / _heartsPerRow;
                var column = heartIndex % _heartsPerRow;
                var x = startX + column * (_heartSize + _heartSpacing);
                var y = startY + row * rowHeight;

                var remaining = filledHalfUnits - heartIndex * 2;
                var texture = remaining >= 2 ? _fullHeartTexture : remaining == 1 ? _halfHeartTexture : _emptyHeartTexture;
                GuiDrawUtils.DrawSlot(new Rect(x, y, _heartSize, _heartSize), texture);
            }

            return startY + rows * rowHeight + 8f;
        }

        public float DrawAmmo(PlayerShoot playerShoot, float y)
        {
            const float startX = 16f;

            if (playerShoot == null || playerShoot.CurrentAmmoType == global::WeaponAmmoType.None)
            {
                return y;
            }

            var (fullTexture, emptyTexture) = GetAmmoTextures(playerShoot.CurrentAmmoType);
            var maxAmmo = playerShoot.MaxAmmo;
            var currentAmmo = playerShoot.CurrentAmmo;
            var rows = Mathf.CeilToInt(maxAmmo / (float)_ammoPerRow);
            var rowHeight = _ammoIconSize + _ammoIconSpacing;

            for (var ammoIndex = 0; ammoIndex < maxAmmo; ammoIndex++)
            {
                var row = ammoIndex / _ammoPerRow;
                var column = ammoIndex % _ammoPerRow;
                var x = startX + column * (_ammoIconSize + _ammoIconSpacing);
                var iconY = y + row * rowHeight;

                var texture = ammoIndex < currentAmmo ? fullTexture : emptyTexture;
                GuiDrawUtils.DrawSlot(new Rect(x, iconY, _ammoIconSize, _ammoIconSize), texture);
            }

            return y + rows * rowHeight + 16f;
        }

        public void DrawReloadBar(PlayerShoot playerShoot)
        {
            if (playerShoot == null || !playerShoot.IsReloading)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var worldPosition = playerShoot.transform.position + Vector3.up * _reloadBarWorldOffset;
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            var rect = new Rect(
                screenPoint.x - _reloadBarWidth / 2f,
                Screen.height - screenPoint.y - _reloadBarHeight / 2f,
                _reloadBarWidth,
                _reloadBarHeight);

            // screenPoint is already a real screen-pixel position from WorldToScreenPoint - draw it
            // outside the reference-resolution scale so it isn't shifted off the player.
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            GuiDrawUtils.DrawSlot(rect, _reloadBarBackgroundTexture);

            var fillRect = new Rect(rect.x, rect.y, rect.width * playerShoot.ReloadFractionRemaining, rect.height);
            GuiDrawUtils.DrawSlot(fillRect, _reloadBarFillTexture);

            GUI.Label(rect, "Reloading", GuiDrawUtils.CenteredLabelStyle);

            GUI.matrix = previousMatrix;
        }

        private (Texture2D full, Texture2D empty) GetAmmoTextures(global::WeaponAmmoType ammoType)
        {
            switch (ammoType)
            {
                case global::WeaponAmmoType.Pistol:
                    return (_pistolAmmoFullTexture, _pistolAmmoEmptyTexture);
                case global::WeaponAmmoType.Shotgun:
                    return (_shotgunAmmoFullTexture, _shotgunAmmoEmptyTexture);
                case global::WeaponAmmoType.Rifle:
                    return (_rifleAmmoFullTexture, _rifleAmmoEmptyTexture);
                default:
                    return (null, null);
            }
        }
    }
}
