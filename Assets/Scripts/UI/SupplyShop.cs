using System;
using System.Collections.Generic;
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
        private enum ShopTab { Buy, Sell }

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

        [Header("Sell — price paid to the player for items sold back, as a fraction of the Item's Value")]
        [SerializeField, Range(0f, 1f)] private float sellPriceMultiplier = 0.5f;

        [Header("Optional GUI art — every field falls back to Unity's basic GUI")]
        [SerializeField] private Texture2D panelGraphic;
        [SerializeField] private Texture2D titleGraphic;
        [SerializeField] private Texture2D itemSlotGraphic;
        [SerializeField] private Texture2D purchaseButtonGraphic;
        [SerializeField] private Texture2D closeButtonGraphic;
        [SerializeField] private Texture2D currencyGraphic;

        // Floats a "-N"/"+N" caps number above whichever Buy/Sell button was just clicked, same
        // rise-and-fade idea as DamageNumberHud but anchored to a screen Rect instead of a world
        // Transform - this panel has no world position of its own to follow.
        private sealed class MoneyPopup
        {
            public string Text;
            public bool IsPositive;
            public float TimeRemaining;
            public Rect AnchorRect;
        }

        private const float MoneyPopupDuration = 1f;
        private const float MoneyPopupRise = 26f;

        private static SupplyShop activeShop;

        private PlayerInventory playerInventory;
        private bool isOpen;
        private ShopTab tab;
        private Vector2 scrollPosition;
        private string statusMessage;
        private GUIStyle centeredLabelStyle;
        private GUIStyle titleLabelStyle;
        private GUIStyle positiveMoneyStyle;
        private GUIStyle negativeMoneyStyle;
        private readonly List<MoneyPopup> moneyPopups = new();

        public static bool IsAnyShopOpen => activeShop != null && activeShop.isOpen;

        // Same base style as every other IMGUI panel, but this one also wraps long item descriptions.
        private GUIStyle CenteredLabelStyle => GuiDrawUtils.GetOrCreate(ref centeredLabelStyle,
            () => new GUIStyle(GuiDrawUtils.CenteredLabelStyle) { wordWrap = true });

        private GUIStyle TitleLabelStyle => GuiDrawUtils.GetOrCreate(ref titleLabelStyle,
            () => new GUIStyle(CenteredLabelStyle) { fontSize = 24, fontStyle = FontStyle.Bold });

        private GUIStyle PositiveMoneyStyle => GuiDrawUtils.GetOrCreate(ref positiveMoneyStyle, () => BuildMoneyStyle(new Color(0.35f, 0.9f, 0.35f)));

        private GUIStyle NegativeMoneyStyle => GuiDrawUtils.GetOrCreate(ref negativeMoneyStyle, () => BuildMoneyStyle(Color.red));

        private GUIStyle BuildMoneyStyle(Color color)
        {
            var style = new GUIStyle(CenteredLabelStyle) { fontStyle = FontStyle.Bold };
            style.normal.textColor = color;
            return style;
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
            tab = ShopTab.Buy;
            scrollPosition = Vector2.zero;
            statusMessage = "Defeat enemies to earn Caps, then restock here.";

            PauseGate.Request(this);
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

            PauseGate.Release(this);
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

            for (var i = moneyPopups.Count - 1; i >= 0; i--)
            {
                moneyPopups[i].TimeRemaining -= Time.deltaTime;
                if (moneyPopups[i].TimeRemaining <= 0f)
                {
                    moneyPopups.RemoveAt(i);
                }
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

            GuiScale.Begin();
            DrawShop();
        }

        private void DrawShop()
        {
            const float panelWidth = 760f;
            const float panelHeight = 560f;
            const float outerPadding = 16f;
            const float titleHeight = 58f;
            const float tabWidth = 120f;
            const float tabHeight = 28f;
            const float rowHeight = 66f;
            const float footerHeight = 48f;

            var width = Mathf.Min(panelWidth, GuiScale.ReferenceWidth - outerPadding * 2f);
            var height = Mathf.Min(panelHeight, GuiScale.ReferenceHeight - outerPadding * 2f);
            var panelRect = new Rect((GuiScale.ReferenceWidth - width) * .5f, (GuiScale.ReferenceHeight - height) * .5f, width, height);

            GuiDrawUtils.DrawSlot(panelRect, panelGraphic);

            var titleRect = new Rect(panelRect.x + outerPadding, panelRect.y + outerPadding, panelRect.width - outerPadding * 2f, titleHeight);
            GuiDrawUtils.DrawSlot(titleRect, titleGraphic);
            GUI.Label(titleRect, shopTitle, TitleLabelStyle);

            var currencyRect = new Rect(titleRect.x + 12f, titleRect.y + 8f, 160f, 28f);
            GuiDrawUtils.DrawSlot(currencyRect, currencyGraphic);
            GUI.Label(currencyRect, $"{currencyName}: {playerInventory.Currency}", CenteredLabelStyle);

            var closeRect = new Rect(titleRect.xMax - 88f, titleRect.y + 8f, 76f, 30f);
            if (DrawButton(closeRect, "Close", closeButtonGraphic))
            {
                Close();
                return;
            }

            var tabY = titleRect.yMax + 6f;
            var buyTabRect = new Rect(titleRect.x, tabY, tabWidth, tabHeight);
            var sellTabRect = new Rect(titleRect.x + tabWidth + 8f, tabY, tabWidth, tabHeight);

            var previousEnabled = GUI.enabled;
            GUI.enabled = tab != ShopTab.Buy;
            if (GUI.Button(buyTabRect, "Buy"))
            {
                SetTab(ShopTab.Buy);
            }
            GUI.enabled = tab != ShopTab.Sell;
            if (GUI.Button(sellTabRect, "Sell"))
            {
                SetTab(ShopTab.Sell);
            }
            GUI.enabled = previousEnabled;

            var footerRect = new Rect(titleRect.x, panelRect.yMax - outerPadding - footerHeight, titleRect.width, footerHeight);
            var listY = tabY + tabHeight + 8f;
            var listArea = new Rect(titleRect.x, listY, titleRect.width, footerRect.y - listY - 6f);

            if (tab == ShopTab.Buy)
            {
                DrawBuyList(listArea, rowHeight);
            }
            else
            {
                DrawSellList(listArea, rowHeight);
            }

            GUI.Label(footerRect, string.IsNullOrWhiteSpace(statusMessage) ? "E / Esc: Close" : statusMessage + "  (E / Esc: Close)", CenteredLabelStyle);

            DrawMoneyPopups();
        }

        private void DrawMoneyPopups()
        {
            if (moneyPopups.Count == 0)
            {
                return;
            }

            var previousColor = GUI.color;

            foreach (var popup in moneyPopups)
            {
                var progress = 1f - Mathf.Clamp01(popup.TimeRemaining / MoneyPopupDuration);
                var rect = new Rect(popup.AnchorRect.x, popup.AnchorRect.y - progress * MoneyPopupRise, popup.AnchorRect.width, popup.AnchorRect.height);

                GUI.color = new Color(1f, 1f, 1f, 1f - progress);
                GuiDrawUtils.DrawLabelWithShadow(rect, popup.Text, popup.IsPositive ? PositiveMoneyStyle : NegativeMoneyStyle);
            }

            GUI.color = previousColor;
        }

        private void SetTab(ShopTab newTab)
        {
            tab = newTab;
            scrollPosition = Vector2.zero;
        }

        // Scrollable rather than sized-to-fit-content: the sell list's length depends on how much the
        // player is carrying (up to 20 slots), which won't reliably fit in a fixed panel height.
        private void DrawBuyList(Rect listArea, float rowHeight)
        {
            if (stock == null || stock.Length == 0)
            {
                GUI.Label(new Rect(listArea.x, listArea.y, listArea.width, rowHeight), "No items are stocked yet.", CenteredLabelStyle);
                return;
            }

            var viewRect = new Rect(0f, 0f, listArea.width - 20f, stock.Length * rowHeight);
            scrollPosition = GUI.BeginScrollView(listArea, scrollPosition, viewRect);

            for (var index = 0; index < stock.Length; index++)
            {
                var rowRect = new Rect(0f, index * rowHeight, viewRect.width, rowHeight - 4f);
                DrawStockRow(rowRect, stock[index]);
            }

            GUI.EndScrollView();
        }

        private void DrawStockRow(Rect rowRect, StockEntry entry)
        {
            GuiDrawUtils.DrawSlot(rowRect, itemSlotGraphic);

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
            var itemName = GuiDrawUtils.GetItemName(entry.item);
            var itemInfoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 7f, rowRect.width - 260f, 24f);
            var stockInfoRect = new Rect(itemInfoRect.x, rowRect.y + 32f, itemInfoRect.width, 22f);
            GUI.Label(itemInfoRect, purchaseAmount > 1 ? $"{itemName} x{purchaseAmount}" : itemName);
            GUI.Label(stockInfoRect, $"{price} {currencyName}  |  {remainingLabel}");

            var canBuy = entry.quantity == 0 || entry.quantity >= purchaseAmount;
            GUI.enabled = canBuy;
            var buyRect = new Rect(rowRect.xMax - 112f, rowRect.y + 12f, 100f, rowRect.height - 24f);
            if (DrawButton(buyRect, "Buy", purchaseButtonGraphic))
            {
                Buy(entry, purchaseAmount, price, buyRect);
            }
            GUI.enabled = true;
        }

        private void Buy(StockEntry entry, int purchaseAmount, int price, Rect buttonRect)
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

            ShowMoneyPopup($"-{price}", isPositive: false, buttonRect);

            var itemName = GuiDrawUtils.GetItemName(entry.item);
            statusMessage = $"Purchased {itemName}{(purchaseAmount > 1 ? $" x{purchaseAmount}" : string.Empty)}.";
        }

        private void ShowMoneyPopup(string text, bool isPositive, Rect anchorRect)
        {
            moneyPopups.Add(new MoneyPopup
            {
                Text = text,
                IsPositive = isPositive,
                TimeRemaining = MoneyPopupDuration,
                AnchorRect = anchorRect
            });
        }

        private int GetPrice(StockEntry entry)
        {
            return entry.price > 0 ? entry.price : Mathf.Max(0, entry.item.value);
        }

        private void DrawSellList(Rect listArea, float rowHeight)
        {
            var slots = playerInventory.Slots;
            var sellableSlotIndices = new List<int>();
            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (!slots[slotIndex].IsEmpty)
                {
                    sellableSlotIndices.Add(slotIndex);
                }
            }

            if (sellableSlotIndices.Count == 0)
            {
                GUI.Label(new Rect(listArea.x, listArea.y, listArea.width, rowHeight), "You have nothing to sell.", CenteredLabelStyle);
                return;
            }

            var viewRect = new Rect(0f, 0f, listArea.width - 20f, sellableSlotIndices.Count * rowHeight);
            scrollPosition = GUI.BeginScrollView(listArea, scrollPosition, viewRect);

            for (var row = 0; row < sellableSlotIndices.Count; row++)
            {
                var slotIndex = sellableSlotIndices[row];
                var rowRect = new Rect(0f, row * rowHeight, viewRect.width, rowHeight - 4f);
                DrawSellRow(rowRect, slotIndex, slots[slotIndex]);
            }

            GUI.EndScrollView();
        }

        private void DrawSellRow(Rect rowRect, int slotIndex, PlayerInventory.InventorySlot slot)
        {
            GuiDrawUtils.DrawSlot(rowRect, itemSlotGraphic);

            var item = slot.Item;
            var iconRect = new Rect(rowRect.x + 8f, rowRect.y + 7f, rowRect.height - 14f, rowRect.height - 14f);
            if (item.icon != null)
            {
                GuiDrawUtils.DrawSprite(iconRect, item.icon);
            }
            else
            {
                GUI.Box(iconRect, "Item");
            }

            var totalPrice = GetSellPrice(item) * slot.Amount;
            var itemName = GuiDrawUtils.GetItemName(item);
            var itemInfoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 7f, rowRect.width - 260f, 24f);
            var priceInfoRect = new Rect(itemInfoRect.x, rowRect.y + 32f, itemInfoRect.width, 22f);
            GUI.Label(itemInfoRect, slot.Amount > 1 ? $"{itemName} x{slot.Amount}" : itemName);
            GUI.Label(priceInfoRect, $"Sells for {totalPrice} {currencyName}");

            var sellRect = new Rect(rowRect.xMax - 112f, rowRect.y + 12f, 100f, rowRect.height - 24f);
            if (DrawButton(sellRect, "Sell", purchaseButtonGraphic))
            {
                Sell(slotIndex, sellRect);
            }
        }

        // Sells the whole stack in one click - selling a large stack of ammo one unit at a time would
        // be tedious, and there's no quantity picker in this UI to ask for a partial amount.
        private void Sell(int slotIndex, Rect buttonRect)
        {
            if (!playerInventory.IsValidSlot(slotIndex))
            {
                return;
            }

            var slot = playerInventory.Slots[slotIndex];
            if (slot.IsEmpty)
            {
                return;
            }

            var item = slot.Item;
            var amount = slot.Amount;
            var totalPrice = GetSellPrice(item) * amount;

            if (!playerInventory.TryRemoveAt(slotIndex, amount))
            {
                return;
            }

            playerInventory.AddCurrency(totalPrice);

            ShowMoneyPopup($"+{totalPrice}", isPositive: true, buttonRect);

            var itemName = GuiDrawUtils.GetItemName(item);
            statusMessage = $"Sold {itemName}{(amount > 1 ? $" x{amount}" : string.Empty)} for {totalPrice} {currencyName}.";
        }

        private int GetSellPrice(global::Item item)
        {
            return Mathf.Max(0, Mathf.RoundToInt(item.value * sellPriceMultiplier));
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
                GuiDrawUtils.DrawSprite(rect, entry.item.icon);
            }
            else
            {
                GUI.Box(rect, "Item");
            }
        }
    }
}
