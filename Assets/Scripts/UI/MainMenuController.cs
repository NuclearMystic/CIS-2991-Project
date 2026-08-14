using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using CIS2991Project.Player;

namespace CIS2991Project.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "Settlement";

        [Header("Supply Shop")]
        [Tooltip("Caps available for purchases made before a player has entered the game.")]
        [SerializeField, Min(0)] private int preGameShopCaps = 60;
        [Tooltip("Items sold by the Supplies Shop button. Quantity 0 means unlimited stock.")]
        [SerializeField] private SupplyShop.StockEntry[] preGameShopStock = Array.Empty<SupplyShop.StockEntry>();

        // Main Menu Buttons
        private Button playButton;
        private Button loadButton;
        private Button shopButton;
        private Button settingsButton;
        private Button quitButton;

        // Settings Menu
        private VisualElement settingsContainer;
        private Button settingsBackButton;

        // Load Menu
        private VisualElement loadContainer;
        private Button loadBackButton;

        // Supply Shop Menu
        private VisualElement shopContainer;
        private ScrollView shopList;
        private Label shopCurrencyLabel;
        private Label shopStatusLabel;
        private Button shopBackButton;

        // UIDocument finishes building its visual tree before Start. Initializing here ensures
        // the menu's shop controls are found and their click callbacks are always registered.
        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            var root = uiDocument.rootVisualElement;

            // ===========================
            // Main Menu Buttons
            // ===========================
            playButton = root.Q<Button>("Play");
            loadButton = root.Q<Button>("Load");
            shopButton = root.Q<Button>("SupplyShop");
            settingsButton = root.Q<Button>("Settings");
            quitButton = root.Q<Button>("Quit");

            // ===========================
            // Settings Menu
            // ===========================
            settingsContainer = root.Q<VisualElement>("SettingsContainer");
            settingsBackButton = root.Q<Button>("BackButton");

            // ===========================
            // Load Menu
            // ===========================
            loadContainer = root.Q<VisualElement>("LoadContainer");
            loadBackButton = root.Q<Button>("LoadBackButton");

            // ===========================
            // Supply Shop Menu
            // ===========================
            shopContainer = root.Q<VisualElement>("ShopContainer");
            shopList = root.Q<ScrollView>("ShopList");
            shopCurrencyLabel = root.Q<Label>("ShopCurrencyLabel");
            shopStatusLabel = root.Q<Label>("ShopStatusLabel");
            shopBackButton = root.Q<Button>("ShopBackButton");

            // ===========================
            // Hide Popups on Startup
            // ===========================
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.None;

            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.None;

            if (shopContainer != null)
                shopContainer.style.display = DisplayStyle.None;

            // ===========================
            // Register Button Events
            // ===========================
            if (playButton != null)
                playButton.clicked += PlayGame;
            else
                Debug.LogError("Play button not found.");

            if (loadButton != null)
                loadButton.clicked += OpenLoad;
            else
                Debug.LogError("Load button not found.");

            if (shopButton != null)
                shopButton.clicked += OpenShop;
            else
                Debug.LogError("Supply Shop button not found.");

            if (settingsButton != null)
                settingsButton.clicked += OpenSettings;
            else
                Debug.LogError("Settings button not found.");

            if (quitButton != null)
                quitButton.clicked += QuitGame;
            else
                Debug.LogError("Quit button not found.");

            if (settingsBackButton != null)
                settingsBackButton.clicked += CloseSettings;
            else
                Debug.LogError("Settings BackButton not found.");

            if (loadBackButton != null)
                loadBackButton.clicked += CloseLoad;
            else
                Debug.LogError("Load BackButton not found.");

            if (shopBackButton != null)
                shopBackButton.clicked += CloseShop;
            else
                Debug.LogError("Supply Shop Back button not found.");
        }

        // ===========================
        // PLAY
        // ===========================
        private void PlayGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        // ===========================
        // SETTINGS MENU
        // ===========================
        private void OpenSettings()
        {
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.Flex;
        }

        private void CloseSettings()
        {
            if (settingsContainer != null)
                settingsContainer.style.display = DisplayStyle.None;
        }

        // ===========================
        // LOAD MENU
        // ===========================
        private void OpenLoad()
        {
            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.Flex;
        }

        private void CloseLoad()
        {
            if (loadContainer != null)
                loadContainer.style.display = DisplayStyle.None;
        }

        // ===========================
        // SUPPLY SHOP
        // ===========================
        private void OpenShop()
        {
            if (shopContainer == null)
            {
                return;
            }

            if (UnityEngine.Object.FindAnyObjectByType<PlayerInventory>() == null)
            {
                MainMenuShopSession.EnsureStarted(preGameShopCaps);
            }
            shopContainer.style.display = DisplayStyle.Flex;
            SetShopStatus("Spend your starting Caps wisely. Defeat enemies to earn more.");
            RenderShop();
        }

        private void CloseShop()
        {
            if (shopContainer != null)
            {
                shopContainer.style.display = DisplayStyle.None;
            }
        }

        private void RenderShop()
        {
            if (shopList == null)
            {
                return;
            }

            shopList.contentContainer.Clear();
            var playerInventory = UnityEngine.Object.FindAnyObjectByType<PlayerInventory>();
            var caps = playerInventory != null ? playerInventory.Currency : MainMenuShopSession.Currency;
            if (shopCurrencyLabel != null)
            {
                shopCurrencyLabel.text = $"Caps: {caps}";
            }

            if (preGameShopStock == null || preGameShopStock.Length == 0)
            {
                shopList.Add(new Label("No items are stocked yet."));
                return;
            }

            foreach (var entry in preGameShopStock)
            {
                shopList.Add(CreateShopRow(entry));
            }
        }

        private VisualElement CreateShopRow(SupplyShop.StockEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("shop-item-row");

            if (entry == null || entry.item == null)
            {
                row.Add(new Label("Assign an Item to this shop entry."));
                return row;
            }

            var icon = new VisualElement();
            icon.AddToClassList("shop-item-icon");
            if (entry.graphic != null)
            {
                icon.style.backgroundImage = new StyleBackground(entry.graphic);
            }
            else if (entry.item.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(entry.item.icon);
            }
            else
            {
                icon.Add(new Label("ITEM"));
            }
            row.Add(icon);

            var details = new VisualElement();
            details.AddToClassList("shop-item-details");
            var purchaseAmount = Mathf.Max(1, entry.amountPerPurchase);
            var itemName = string.IsNullOrWhiteSpace(entry.item.displayName) ? entry.item.name : entry.item.displayName;
            var itemNameLabel = new Label(purchaseAmount > 1 ? $"{itemName} x{purchaseAmount}" : itemName);
            itemNameLabel.AddToClassList("shop-item-name");
            details.Add(itemNameLabel);

            var price = GetPrice(entry);
            var stockLabel = entry.quantity > 0 ? $"Stock: {entry.quantity}" : "Stock: Unlimited";
            var priceLabel = new Label($"{price} Caps  |  {stockLabel}");
            priceLabel.AddToClassList("shop-item-price");
            details.Add(priceLabel);
            row.Add(details);

            var canBuy = entry.quantity == 0 || entry.quantity >= purchaseAmount;
            var buyButton = new Button(() => TryBuy(entry)) { text = canBuy ? "Buy" : "Sold Out" };
            buyButton.AddToClassList("shop-buy-button");
            buyButton.SetEnabled(canBuy);
            row.Add(buyButton);
            return row;
        }

        private void TryBuy(SupplyShop.StockEntry entry)
        {
            if (entry == null || entry.item == null)
            {
                SetShopStatus("This shop entry needs an Item assigned.");
                return;
            }

            var amount = Mathf.Max(1, entry.amountPerPurchase);
            var price = GetPrice(entry);
            if (entry.quantity > 0 && entry.quantity < amount)
            {
                SetShopStatus("That item is sold out.");
                RenderShop();
                return;
            }

            var playerInventory = UnityEngine.Object.FindAnyObjectByType<PlayerInventory>();
            if (playerInventory != null)
            {
                if (!playerInventory.CanAdd(entry.item, amount))
                {
                    SetShopStatus("Your inventory is full.");
                    return;
                }

                if (!playerInventory.TrySpendCurrency(price))
                {
                    SetShopStatus($"You need {price} Caps.");
                    return;
                }

                if (!playerInventory.TryAdd(entry.item, amount))
                {
                    playerInventory.AddCurrency(price);
                    SetShopStatus("Purchase failed: your inventory is full.");
                    return;
                }
            }
            else if (!MainMenuShopSession.TryPurchase(entry.item, amount, price, preGameShopCaps))
            {
                SetShopStatus($"You need {price} Caps.");
                return;
            }

            if (entry.quantity > 0)
            {
                entry.quantity -= amount;
            }

            var itemName = string.IsNullOrWhiteSpace(entry.item.displayName) ? entry.item.name : entry.item.displayName;
            SetShopStatus($"Purchased {itemName}{(amount > 1 ? $" x{amount}" : string.Empty)}.");
            RenderShop();
        }

        private static int GetPrice(SupplyShop.StockEntry entry)
        {
            return entry.price > 0 ? entry.price : Mathf.Max(0, entry.item.value);
        }

        private void SetShopStatus(string message)
        {
            if (shopStatusLabel != null)
            {
                shopStatusLabel.text = message;
            }
        }

        // ===========================
        // QUIT
        // ===========================
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
