using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Time.timeScale has exactly one live value, but SupplyShop, PauseMenuController, and
    // GameOverController each independently want to freeze/unfreeze it. Tracking the set of active
    // requesters means whichever one releases last is the one that actually restores timeScale, so
    // two of them being open at once can't stomp on each other's expected restore value.
    public static class PauseGate
    {
        private static readonly HashSet<object> Owners = new();

        public static void Request(object owner)
        {
            Owners.Add(owner);
            Time.timeScale = 0f;
        }

        public static void Release(object owner)
        {
            Owners.Remove(owner);
            if (Owners.Count == 0)
            {
                Time.timeScale = 1f;
            }
        }

        // Leaving gameplay entirely (returning to the main menu) - any owner still registered belongs
        // to the scene being left behind, so clear the slate instead of leaving a stale registration
        // that would keep timeScale stuck at 0 forever (e.g. the pause menu was opened but never
        // explicitly closed before the run ended).
        public static void ResetAll()
        {
            Owners.Clear();
            Time.timeScale = 1f;
        }
    }
}
