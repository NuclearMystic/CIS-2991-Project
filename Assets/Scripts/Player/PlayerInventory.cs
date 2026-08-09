using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        public const int DefaultSlotCount = 20;

        [Serializable]
        public class InventorySlot
        {
            [SerializeField] private global::Item item;
            [SerializeField, Min(0)] private int amount;

            public global::Item Item => item;
            public int Amount => amount;
            public bool IsEmpty => item == null || amount <= 0;
            public int MaxStackSize => GetMaxStackSize(item);
            public int SpaceRemaining => IsEmpty ? 0 : Mathf.Max(MaxStackSize - amount, 0);

            public bool CanStackWith(global::Item candidate)
            {
                return !IsEmpty &&
                       item.stackable &&
                       ItemsMatch(item, candidate) &&
                       SpaceRemaining > 0;
            }

            internal void Set(global::Item newItem, int newAmount)
            {
                item = newItem;
                amount = Mathf.Max(newAmount, 0);

                if (amount <= 0)
                {
                    Clear();
                }
            }

            internal int Add(int amountToAdd)
            {
                var acceptedAmount = Mathf.Min(amountToAdd, SpaceRemaining);
                amount += acceptedAmount;
                return acceptedAmount;
            }

            internal void Remove(int amountToRemove)
            {
                amount -= amountToRemove;

                if (amount <= 0)
                {
                    Clear();
                }
            }

            internal void Clear()
            {
                item = null;
                amount = 0;
            }
        }

        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>(DefaultSlotCount);
        [SerializeField] private global::Item equippedWeapon;
        [SerializeField] private global::Item equippedArmor;

        private PlayerHealth playerHealth;
        private string lastMessage;

        public event Action InventoryChanged;
        public event Action EquipmentChanged;
        public event Action<global::Item, int> ItemDropped;
        public event Action<string> InventoryMessageChanged;

        public IReadOnlyList<InventorySlot> Slots => slots;
        public int SlotCount => DefaultSlotCount;
        public global::Item EquippedWeapon => equippedWeapon;
        public global::Item EquippedArmor => equippedArmor;
        public string LastMessage => lastMessage;
        public int OccupiedSlotCount => CountOccupiedSlots();
        public int EmptySlotCount => DefaultSlotCount - OccupiedSlotCount;

        private void Awake()
        {
            EnsureSlotCount();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void OnValidate()
        {
            EnsureSlotCount();
        }

        public bool Add(global::Item item, int amount = 1)
        {
            return TryAdd(item, amount);
        }

        public bool TryAdd(global::Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            EnsureSlotCount();

            if (!CanAdd(item, amount))
            {
                SetMessage("Inventory full.");
                return false;
            }

            var remainingAmount = amount;

            if (item.stackable)
            {
                for (var slotIndex = 0; slotIndex < slots.Count && remainingAmount > 0; slotIndex++)
                {
                    var slot = slots[slotIndex];
                    if (!slot.CanStackWith(item))
                    {
                        continue;
                    }

                    remainingAmount -= slot.Add(remainingAmount);
                }
            }

            for (var slotIndex = 0; slotIndex < slots.Count && remainingAmount > 0; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (!slot.IsEmpty)
                {
                    continue;
                }

                var amountForSlot = item.stackable
                    ? Mathf.Min(GetMaxStackSize(item), remainingAmount)
                    : 1;

                slot.Set(item, amountForSlot);
                remainingAmount -= amountForSlot;
            }

            SetMessage(string.Empty);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool CanAdd(global::Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            EnsureSlotCount();

            var remainingAmount = amount;

            if (item.stackable)
            {
                for (var slotIndex = 0; slotIndex < slots.Count && remainingAmount > 0; slotIndex++)
                {
                    var slot = slots[slotIndex];
                    if (slot.CanStackWith(item))
                    {
                        remainingAmount -= Mathf.Min(slot.SpaceRemaining, remainingAmount);
                    }
                }
            }

            for (var slotIndex = 0; slotIndex < slots.Count && remainingAmount > 0; slotIndex++)
            {
                if (!slots[slotIndex].IsEmpty)
                {
                    continue;
                }

                remainingAmount -= item.stackable
                    ? Mathf.Min(GetMaxStackSize(item), remainingAmount)
                    : 1;
            }

            return remainingAmount <= 0;
        }

        public bool TryRemoveAt(int slotIndex, int amount = 1)
        {
            if (!IsValidSlot(slotIndex) || amount <= 0)
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.Amount < amount)
            {
                return false;
            }

            var removedItem = slot.Item;
            slot.Remove(amount);
            RefreshEquipmentAfterItemRemoved(removedItem);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryRemove(global::Item item, int amount = 1)
        {
            if (!CanRemove(item, amount))
            {
                return false;
            }

            var remainingAmount = amount;
            for (var slotIndex = 0; slotIndex < slots.Count && remainingAmount > 0; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (slot.IsEmpty || !ItemsMatch(slot.Item, item))
                {
                    continue;
                }

                var amountToRemove = Mathf.Min(slot.Amount, remainingAmount);
                slot.Remove(amountToRemove);
                remainingAmount -= amountToRemove;
            }

            RefreshEquipmentAfterItemRemoved(item);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool CanRemove(global::Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            EnsureSlotCount();

            var foundAmount = 0;
            for (var slotIndex = 0; slotIndex < slots.Count && foundAmount < amount; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (!slot.IsEmpty && ItemsMatch(slot.Item, item))
                {
                    foundAmount += slot.Amount;
                }
            }

            return foundAmount >= amount;
        }

        public bool TryDropAt(int slotIndex, int amount = 1)
        {
            if (!IsValidSlot(slotIndex) || amount <= 0)
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.Amount < amount)
            {
                return false;
            }

            var droppedItem = slot.Item;
            slot.Remove(amount);
            RefreshEquipmentAfterItemRemoved(droppedItem);
            InventoryChanged?.Invoke();
            ItemDropped?.Invoke(droppedItem, amount);
            return true;
        }

        public bool TryUseAt(int slotIndex, PlayerHealth healthOverride = null)
        {
            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.Item.itemType != global::ItemType.Consumable)
            {
                return false;
            }

            var item = slot.Item;
            var targetHealth = healthOverride != null ? healthOverride : playerHealth;
            if (item.healthRestore > 0)
            {
                if (targetHealth == null)
                {
                    return false;
                }

                targetHealth.Heal(item.healthRestore);
            }

            slot.Remove(1);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryEquipAt(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            var slot = slots[slotIndex];
            if (slot.IsEmpty || !IsEquipment(slot.Item))
            {
                return false;
            }

            if (slot.Item.itemType == global::ItemType.Weapon)
            {
                equippedWeapon = slot.Item;
            }
            else if (slot.Item.itemType == global::ItemType.Armor)
            {
                equippedArmor = slot.Item;
            }

            EquipmentChanged?.Invoke();
            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryEquip(global::Item item)
        {
            if (item == null)
            {
                return false;
            }

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (!slot.IsEmpty && ItemsMatch(slot.Item, item))
                {
                    return TryEquipAt(slotIndex);
                }
            }

            return false;
        }

        public bool HasItem(global::Item item)
        {
            if (item == null)
            {
                return false;
            }

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (!slots[slotIndex].IsEmpty && ItemsMatch(slots[slotIndex].Item, item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanUseAt(int slotIndex)
        {
            return IsValidSlot(slotIndex) &&
                   !slots[slotIndex].IsEmpty &&
                   slots[slotIndex].Item.itemType == global::ItemType.Consumable;
        }

        public bool CanEquipAt(int slotIndex)
        {
            return IsValidSlot(slotIndex) &&
                   !slots[slotIndex].IsEmpty &&
                   IsEquipment(slots[slotIndex].Item);
        }

        public int FindStackWithSpace(global::Item item)
        {
            if (item == null || !item.stackable)
            {
                return -1;
            }

            EnsureSlotCount();

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (slots[slotIndex].CanStackWith(item))
                {
                    return slotIndex;
                }
            }

            return -1;
        }

        public int FindEmptySlot()
        {
            EnsureSlotCount();

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (slots[slotIndex].IsEmpty)
                {
                    return slotIndex;
                }
            }

            return -1;
        }

        public bool IsValidSlot(int slotIndex)
        {
            EnsureSlotCount();
            return slotIndex >= 0 && slotIndex < slots.Count;
        }

        public static bool IsEquipment(global::Item item)
        {
            return item != null &&
                   (item.itemType == global::ItemType.Weapon ||
                    item.itemType == global::ItemType.Armor);
        }

        private int CountOccupiedSlots()
        {
            EnsureSlotCount();

            var occupiedSlots = 0;
            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (!slots[slotIndex].IsEmpty)
                {
                    occupiedSlots++;
                }
            }

            return occupiedSlots;
        }

        private void EnsureSlotCount()
        {
            if (slots == null)
            {
                slots = new List<InventorySlot>(DefaultSlotCount);
            }

            while (slots.Count < DefaultSlotCount)
            {
                slots.Add(new InventorySlot());
            }

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (slots[slotIndex] == null)
                {
                    slots[slotIndex] = new InventorySlot();
                }
            }

            while (slots.Count > DefaultSlotCount)
            {
                slots.RemoveAt(slots.Count - 1);
            }
        }

        private void SetMessage(string message)
        {
            if (lastMessage == message)
            {
                return;
            }

            lastMessage = message;
            InventoryMessageChanged?.Invoke(lastMessage);
        }

        private void RefreshEquipmentAfterItemRemoved(global::Item removedItem)
        {
            var changedEquipment = false;

            if (ItemsMatch(equippedWeapon, removedItem) && !CanRemove(equippedWeapon, 1))
            {
                equippedWeapon = null;
                changedEquipment = true;
            }

            if (ItemsMatch(equippedArmor, removedItem) && !CanRemove(equippedArmor, 1))
            {
                equippedArmor = null;
                changedEquipment = true;
            }

            if (changedEquipment)
            {
                EquipmentChanged?.Invoke();
            }
        }

        private static int GetMaxStackSize(global::Item item)
        {
            if (item == null)
            {
                return 0;
            }

            return item.stackable ? Mathf.Max(1, item.maxStackSize) : 1;
        }

        private static bool ItemsMatch(global::Item firstItem, global::Item secondItem)
        {
            if (firstItem == null || secondItem == null)
            {
                return false;
            }

            if (ReferenceEquals(firstItem, secondItem))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(firstItem.id) &&
                !string.IsNullOrWhiteSpace(secondItem.id) &&
                string.Equals(firstItem.id, secondItem.id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(firstItem.displayName) &&
                   !string.IsNullOrWhiteSpace(secondItem.displayName) &&
                   string.Equals(firstItem.displayName, secondItem.displayName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
