using System.Collections.Generic;
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
    public sealed class ChestInventory : MonoBehaviour
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
        public static ChestInventory ActiveChest { get; private set; }

        private readonly List<PlayerInventory.InventorySlot> slots = new();
        private SpriteRenderer _renderer;
        private bool _playerInRange;
        private bool _isOpen;

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

        private void OnDisable()
        {
            if (ActiveChest == this)
                Close();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerInventory>() != null)
                _playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerInventory>() == null)
                return;

            _playerInRange = false;
            if (ActiveChest == this)
                Close();
        }

        private void Update()
        {
            if (!_playerInRange || !Input.GetKeyDown(interactKey))
                return;

            if (ActiveChest == this)
                Close();
            else
                Open();
        }

        private void Open()
        {
            if (ActiveChest != null && ActiveChest != this)
                ActiveChest.Close();

            _isOpen = true;
            ActiveChest = this;
            UpdateSprite();
        }

        public void Close()
        {
            _isOpen = false;
            if (ActiveChest == this)
                ActiveChest = null;
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

            var sprite = _isOpen ? openSprite : closedSprite;
            // Fallback so the chest is visible/testable before closedSprite/openSprite are set -
            // same runtime-generated-solid-sprite pattern already used elsewhere in this project
            // (e.g. SettlementSceneLayout.Solid).
            _renderer.sprite = sprite != null ? sprite : Fallback(_isOpen ? Color.yellow : new Color(0.45f, 0.28f, 0.1f));
        }

        private static Sprite Fallback(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
    }
}
