namespace CIS2991Project.UI
{
    // Shared between InventoryPanel (starts a drag on mouse-down over an eligible slot) and HotbarHud
    // (consumes it on mouse-up over a hotbar slot). PlayerHUD owns the single instance and resets it
    // once, after every panel has had a chance to read it this frame.
    public sealed class DragState
    {
        public int SlotIndex = -1;
    }
}
