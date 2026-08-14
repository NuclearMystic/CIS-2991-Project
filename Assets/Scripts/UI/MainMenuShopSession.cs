using System.Collections.Generic;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    /// <summary>
    /// Holds main-menu purchases until the first player is created. This makes the menu shop
    /// useful before gameplay exists, while keeping the real PlayerInventory as the source of
    /// truth once a game has started.
    /// </summary>
    public static class MainMenuShopSession
    {
        private struct PendingPurchase
        {
            public global::Item item;
            public int amount;
            public int price;
        }

        private static readonly List<PendingPurchase> pendingPurchases = new();
        private static bool isStarted;
        private static int currency;

        public static int Currency => currency;

        public static void EnsureStarted(int startingCaps)
        {
            if (isStarted)
            {
                return;
            }

            currency = Mathf.Max(0, startingCaps);
            isStarted = true;
        }

        public static bool TryPurchase(global::Item item, int amount, int price, int startingCaps)
        {
            if (item == null || amount <= 0 || price < 0)
            {
                return false;
            }

            EnsureStarted(startingCaps);
            if (currency < price)
            {
                return false;
            }

            currency -= price;
            pendingPurchases.Add(new PendingPurchase { item = item, amount = amount, price = price });
            return true;
        }

        public static void ApplyPendingPurchases(PlayerInventory inventory)
        {
            if (!isStarted || inventory == null)
            {
                return;
            }

            inventory.SetCurrency(currency);
            foreach (var purchase in pendingPurchases)
            {
                if (!inventory.TryAdd(purchase.item, purchase.amount))
                {
                    // A future loadout change might fill every slot. Refund instead of silently
                    // discarding a pre-game purchase.
                    inventory.AddCurrency(purchase.price);
                    Debug.LogWarning($"Could not add {purchase.item.name} from the main-menu shop. Caps were refunded.");
                }
            }

            pendingPurchases.Clear();
            isStarted = false;
        }
    }
}
