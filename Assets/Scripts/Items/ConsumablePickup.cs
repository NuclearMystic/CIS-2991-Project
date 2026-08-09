using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class ConsumablePickup : MonoBehaviour
    {
        [SerializeField] private global::Item item;
        [SerializeField, Min(1)] private int amount = 1;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Configure(global::Item pickupItem, int pickupAmount = 1)
        {
            // Runtime scene builders use this to turn PostApocalypse props into inventory loot.
            item = pickupItem;
            amount = Mathf.Max(1, pickupAmount);
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (item == null)
            {
                return;
            }

            var inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null)
            {
                return;
            }

            if (inventory.TryAdd(item, amount))
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log($"Inventory full. Could not pick up {item.displayName}.", this);
        }
    }
}
