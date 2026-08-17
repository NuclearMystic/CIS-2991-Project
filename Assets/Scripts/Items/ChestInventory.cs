using System.Collections.Generic;
using CIS2991Project.Core;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Items
{
    // World loot container. Contents are pre-authored per instance, each with a min/max amount
    // range rolled on spawn (global::ItemDrop, shared with EnemyDefinition's loot table) rather than
    // a fixed quantity. Reuses PlayerInventory's own nested InventorySlot type for storage instead
    // of duplicating that data shape.
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ChestInventory : RangeInteractable<ChestInventory>
    {
        [SerializeField] private List<global::ItemDrop> contents = new();
        [Tooltip("How many items (picked from Contents, with replacement) this chest holds - a " +
                 "random amount between 1 and this, not necessarily one of each.")]
        [SerializeField, Min(1)] private int maxItemsPicked = 6;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        // PlayerHUD reads this each frame to decide whether to draw the chest panel - avoids
        // needing to discover procedurally-spawned chest instances individually.
        public static ChestInventory ActiveChest => Active;

        private readonly List<PlayerInventory.InventorySlot> slots = new();
        private SpriteRenderer _renderer;

        public IReadOnlyList<PlayerInventory.InventorySlot> Slots => slots;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            GetComponent<Collider2D>().isTrigger = true;

            PopulateRandomContents();

            UpdateSprite();
        }

        // Rolls a random assortment rather than always containing every possible entry: pick how
        // many items this chest holds (1 to maxItemsPicked, capped by how many entries actually
        // exist to pick from), then independently pick that many entries from contents (with
        // replacement, so the same entry can be picked more than once and end up as separate stacks).
        private void PopulateRandomContents()
        {
            if (contents.Count == 0)
                return;

            var pickCount = Random.Range(1, Mathf.Min(maxItemsPicked, contents.Count) + 1);
            for (var i = 0; i < pickCount; i++)
            {
                var drop = contents[Random.Range(0, contents.Count)];
                if (drop.item == null)
                    continue;

                var slot = new PlayerInventory.InventorySlot();
                slot.Set(drop.item, drop.RollAmount());
                slots.Add(slot);
            }
        }

        private void Update()
        {
            if (!PlayerInRange || !Input.GetKeyDown(interactKey))
                return;

            if (IsOpen)
                Close();
            else
                Open();
        }

        private void Open()
        {
            BecomeActive();
            UpdateSprite();
        }

        public override void Close()
        {
            base.Close();
            UpdateSprite();
        }

        // Called by PlayerHUD once it's successfully handed this slot's contents to the player.
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            slots[slotIndex].Clear();
        }

        private void UpdateSprite()
        {
            if (_renderer == null)
                return;

            var sprite = IsOpen ? openSprite : closedSprite;
            // Fallback so the chest is visible/testable before closedSprite/openSprite are set.
            _renderer.sprite = sprite != null ? sprite : RuntimeSpriteUtils.CreateSolidSprite(IsOpen ? Color.yellow : new Color(0.45f, 0.28f, 0.1f));
        }
    }
}
