using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    /// <summary>
    /// Opens the linked <see cref="SupplyShop"/> the instant the player walks through this
    /// trigger. Place it set back inside a building doorway (so the player has to actually
    /// step through the door to reach it) and size its BoxCollider2D to match the door art.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class SupplyShopDoor : MonoBehaviour
    {
        [SerializeField] private SupplyShop shop;

        private void Reset()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            shop = GetComponent<SupplyShop>();
        }

        private void Awake()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;

            if (shop == null)
            {
                shop = GetComponent<SupplyShop>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (shop == null || SupplyShop.IsAnyShopOpen)
            {
                return;
            }

            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            if (playerInventory != null)
            {
                shop.Open(playerInventory);
            }
        }
    }
}
