using System;
using System.Collections.Generic;
using CIS2991Project.Data;
using UnityEngine;

namespace CIS2991Project.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [Serializable]
        public class ConsumableStack
        {
            public ConsumableItemDefinition item;
            public int amount;
        }

        [SerializeField] private List<ConsumableStack> consumables = new List<ConsumableStack>();

        public event Action InventoryChanged;

        public IReadOnlyList<ConsumableStack> Consumables => consumables;

        public void Add(ConsumableItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return;
            }

            var stack = consumables.Find(entry => entry.item == item);
            if (stack == null)
            {
                stack = new ConsumableStack { item = item, amount = 0 };
                consumables.Add(stack);
            }

            stack.amount += amount;
            InventoryChanged?.Invoke();
        }

        public bool TryConsume(ConsumableItemDefinition item, PlayerHealth health)
        {
            if (item == null || health == null)
            {
                return false;
            }

            var stack = consumables.Find(entry => entry.item == item);
            if (stack == null || stack.amount <= 0)
            {
                return false;
            }

            stack.amount--;
            if (stack.amount <= 0)
            {
                consumables.Remove(stack);
            }

            health.Heal(item.HealAmount);
            InventoryChanged?.Invoke();
            return true;
        }
    }
}
