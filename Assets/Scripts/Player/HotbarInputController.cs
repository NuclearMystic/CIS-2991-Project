using UnityEngine;

namespace CIS2991Project.Player
{
    // Reads number-key (1-8) input and triggers the matching hotbar slot on PlayerInventory. Kept
    // separate from PlayerInventory itself, which is otherwise a plain data/logic model with no other
    // direct Input.* polling.
    public sealed class HotbarInputController : MonoBehaviour
    {
        private PlayerInventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (_inventory == null)
            {
                return;
            }

            for (var hotbarIndex = 0; hotbarIndex < PlayerInventory.HotbarSlotCount; hotbarIndex++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + hotbarIndex))
                {
                    _inventory.TryUseHotbarSlot(hotbarIndex);
                }
            }
        }
    }
}
