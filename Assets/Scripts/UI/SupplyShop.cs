using System;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    /// <summary>
    /// A small IMGUI shop. It is opened by <see cref="SupplyShopDoor"/> and keeps all of its
    /// stock, prices, and optional UI art on the same scene object for simple Inspector authoring.
    /// </summary>
    public sealed class SupplyShop : MonoBehaviour
    {
        [Serializable]
        public class StockEntry
        {
            [Tooltip("The Item asset this shop sells.")]
            public global::Item item;

            [Tooltip("How many are delivered by one purchase (useful for ammo bundles).")]
            [Min(1)] public int amountPerPurchase = 1;

            [Tooltip("0 means the item has unlimited stock. Otherwise this is the total number of items remaining, not bundles.")]
            [Min(0)] public int quantity;

            [Tooltip("The price of one purchase. Set this to 0 to use the Item asset's Value field.")]
            [Min(0)] public int price;

            [Header("Optional entry art")]
            [Tooltip("Overrides the Item icon for this shop entry. Leave empty to use the Item icon, then the plain GUI fallback.")]
            public Texture2D graphic;
        }

        [Header("Shop details")]
        [SerializeField] private string shopTitle = "Supplies";
        [SerializeField] private string currencyName = "Caps";
        [SerializeField] private StockEntry[] stock = Array.Empty<StockEntry>();

        [Header("Optional GUI art — every field falls back to Unity's basic GUI")]
        [SerializeField] private Texture2D panelGraphic;
        [SerializeField] private Texture2D titleGraphic;
        [SerializeField] private Texture2D itemSlotGraphic;
        [SerializeField] private Texture2D purchaseButtonGraphic;
        [SerializeField] private Texture2D closeButtonGraphic;
        [SerializeField] private Texture2D currencyGraphic;

        private static SupplyShop activeShop;

        private PlayerInventory playerInventory;
        private bool isOpen;
        private bool pausedByShop;
        private float timeScaleBeforeOpen = 1f;
        private string statusMessage;
        private GUIStyle centeredLabelStyle;
        private GUIStyle titleLabelStyle;

        public static bool IsAnyShopOpen => activeShop != null && activeShop.isOpen;

        private GUIStyle CenteredLabelStyle
        {
            get
            {
                if (centeredLabelStyle == null)
                {
                    centeredLabelStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true
                    };
                }

                return centeredLabelStyle;
            }
        }

        private GUIStyle TitleLabelStyle
        {
            get
            {
                if (titleLabelStyle == null)
                {
                    titleLabelStyle = new GUIStyle(CenteredLabelStyle)
                    {
                        fontSize = 24,
                        fontStyle = FontStyle.Bold
                    };
                }

                return titleLabelStyle;
            }
        }

        public void Open(PlayerInventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogWarning($"{name}: A player inventory is required to open this shop.", this);
                return;
            }

            if (activeShop != null && activeShop != this)
            {
                activeShop.Close();
            }

            playerInventory = inventory;
            isOpen = true;
            activeShop = this;
            statusMessage = "Defeat enemies to earn Caps, then restock here.";

            if (!pausedByShop)
            {
                timeScaleBeforeOpen = Time.timeScale;
                Time.timeScale = 0f;
                pausedByShop = true;
            }
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            if (activeShop == this)
            {
                activeShop = null;
            }

            if (pausedByShop)
            {
                Time.timeScale = timeScaleBeforeOpen;
                pausedByShop = false;
            }
        }

        public static void CloseActiveShop()
        {
            if (activeShop != null)
            {
                activeShop.Close();
            }
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(KeyCode.E))
            {
                Close();
            }
        }

        private void OnDisable()
        {
            if (activeShop == this)
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if (!isOpen || playerInventory == null)
            {
                return;
            }

            DrawShop();
        }

        private void DrawShop()
        {
            const float panelWidth = 760f;
            const float outerPadding = 16f;
            const float titleHeight = 58f;
            const float rowHeight = 66f;
            const float footerHeight = 48f;

            var rowCount = Mathf.Max(stock != null ? stock.Length : 0, 1);
            var desiredHeight = titleHeight + rowCount * rowHeight + footerHeight + outerPadding * 2f;
            var width = Mathf.Min(panelWidth, Screen.width - outerPadding * 2f);
            var height = Mathf.Min(desiredHeight, Screen.height - outerPadding * 2f);
            var panelRect = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);

            DrawFrame(panelRect, panelGraphic);

            var titleRect = new Rect(panelRect.x + outerPadding, panelRect.y + outerPadding, panelRect.width - outerPadding * 2f, titleHeight);
            DrawFrame(titleRect, titleGraphic);
            GUI.Label(titleRect, shopTitle, TitleLabelStyle);

            var currencyRect = new Rect(titleRect.x + 12f, titleRect.y + 8f, 160f, 28f);
            DrawFrame(currencyRect, currencyGraphic);
            GUI.Label(currencyRect, $"{currencyName}: {playerInventory.Currency}", CenteredLabelStyle);

            var closeRect = new Rect(titleRect.xMax - 88f, titleRect.y + 8f, 76f, 30f);
            if (DrawButton(closeRect, "Close", closeButtonGraphic))
            {
                Close();
                return;
            }

            var listY = titleRect.yMax + 6f;
            if (stock == null || stock.Length == 0)
            {
                GUI.Label(new Rect(titleRect.x, listY, titleRect.width, rowHeight), "No items are stocked yet.", CenteredLabelStyle);
            }
            else
            {
                for (var index = 0; index < stock.Length; index++)
                {
                    var rowRect = new Rect(titleRect.x, listY + index * rowHeight, titleRect.width, rowHeight - 4f);
                    DrawStockRow(rowRect, stock[index]);
                }
            }

            var footerRect = new Rect(titleRect.x, panelRect.yMax - outerPadding - footerHeight, titleRect.width, footerHeight);
            GUI.Label(footerRect, string.IsNullOrWhiteSpace(statusMessage) ? "E / Esc: Close" : statusMessage + "  (E / Esc: Close)", CenteredLabelStyle);
        }

        private void DrawStockRow(Rect rowRect, StockEntry entry)
        {
            DrawFrame(rowRect, itemSlotGraphic);

            if (entry == null || entry.item == null)
            {
                GUI.Label(rowRect, "Assign an Item to this shop entry.", CenteredLabelStyle);
                return;
            }

            var iconRect = new Rect(rowRect.x + 8f, rowRect.y + 7f, rowRect.height - 14f, rowRect.height - 14f);
            DrawItemGraphic(iconRect, entry);

            var purchaseAmount = Mathf.Max(1, entry.amountPerPurchase);
            var price = GetPrice(entry);
            var remainingLabel = entry.quantity > 0 ? $"Stock: {entry.quantity}" : "Stock: Unlimited";
            var itemName = string.IsNullOrWhiteSpace(entry.item.displayName) ? entry.item.name : entry.item.displayName;
            var itemInfoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 7f, rowRect.width - 260f, 24f);
            var stockInfoRect = new Rect(itemInfoRect.x, rowRect.y + 32f, itemInfoRect.width, 22f);
            GUI.Label(itemInfoRect, purchaseAmount > 1 ? $"{itemName} x{purchaseAmount}" : itemName);
            GUI.Label(stockInfoRect, $"{price} {currencyName}  |  {remainingLabel}");

            var canBuy = entry.quantity == 0 || entry.quantity >= purchaseAmount;
            GUI.enabled = canBuy;
            var buyRect = new Rect(rowRect.xMax - 112f, rowRect.y + 12f, 100f, rowRect.height - 24f);
            if (DrawButton(buyRect, "Buy", purchaseButtonGraphic))
            {
                Buy(entry, purchaseAmount, price);
            }
            GUI.enabled = true;
        }

        private void Buy(StockEntry entry, int purchaseAmount, int price)
        {
            if (entry.quantity > 0 && entry.quantity < purchaseAmount)
            {
                statusMessage = "That item is out of stock.";
                return;
            }

            if (!playerInventory.CanAdd(entry.item, purchaseAmount))
            {
                statusMessage = "Your inventory is full.";
                return;
            }

            if (!playerInventory.TrySpendCurrency(price))
            {
                statusMessage = $"You need {price} {currencyName}.";
                return;
            }

            if (!playerInventory.TryAdd(entry.item, purchaseAmount))
            {
                playerInventory.AddCurrency(price);
                statusMessage = "Purchase failed: your inventory is full.";
                return;
            }

            if (entry.quantity > 0)
            {
                entry.quantity -= purchaseAmount;
            }

            var itemName = string.IsNullOrWhiteSpace(entry.item.displayName) ? entry.item.name : entry.item.displayName;
            statusMessage = $"Purchased {itemName}{(purchaseAmount > 1 ? $" x{purchaseAmount}" : string.Empty)}.";
        }

        private int GetPrice(StockEntry entry)
        {
            return entry.price > 0 ? entry.price : Mathf.Max(0, entry.item.value);
        }

        private static void DrawFrame(Rect rect, Texture2D graphic)
        {
            if (graphic != null)
            {
                GUI.DrawTexture(rect, graphic, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Box(rect, GUIContent.none);
            }
        }

        private bool DrawButton(Rect rect, string label, Texture2D graphic)
        {
            var clicked = GUI.Button(rect, graphic == null ? label : string.Empty);
            if (graphic != null)
            {
                GUI.DrawTexture(rect, graphic, ScaleMode.ScaleToFit);
                GUI.Label(rect, label, CenteredLabelStyle);
            }

            return clicked;
        }

        private static void DrawItemGraphic(Rect rect, StockEntry entry)
        {
            if (entry.graphic != null)
            {
                GUI.DrawTexture(rect, entry.graphic, ScaleMode.ScaleToFit);
            }
            else if (entry.item.icon != null)
            {
                var texture = entry.item.icon.texture;
                var textureRect = entry.item.icon.textureRect;
                var uv = new Rect(
                    textureRect.x / texture.width,
                    textureRect.y / texture.height,
                    textureRect.width / texture.width,
                    textureRect.height / texture.height);
                GUI.DrawTextureWithTexCoords(rect, texture, uv);
            }
            else
            {
                GUI.Box(rect, "Item");
            }
        }
    }
}
