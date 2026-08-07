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
