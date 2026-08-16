using UnityEngine;

namespace CIS2991Project.UI
{
    // Shared by EquipmentHud and InventoryPanel so a click on one right after a click on the other
    // never reads as a double-click pair - both register against one shared last-click id/time, the
    // same way PlayerHUD used to with its sentinel WeaponBoxClickId/OutfitBoxClickId constants.
    public sealed class DoubleClickTracker
    {
        private const double DoubleClickSeconds = 0.3;

        private int _lastClickId = int.MinValue;
        private double _lastClickTime = -1d;

        public bool RegisterClick(int id)
        {
            var now = Time.unscaledTimeAsDouble;
            if (_lastClickId == id && now - _lastClickTime <= DoubleClickSeconds)
            {
                _lastClickId = int.MinValue;
                return true;
            }

            _lastClickId = id;
            _lastClickTime = now;
            return false;
        }
    }
}
