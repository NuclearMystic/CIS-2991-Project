using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Core
{
    // Shared by ChestInventory and NpcDialogue: both are "walk into range, then interact" world
    // objects where only one instance of that same type can be open at a time (opening one closes
    // whichever other instance of T was open). T is the concrete subclass, so a chest and an NPC
    // dialogue can be open simultaneously - just not two chests.
    //
    // What's NOT shared here, deliberately: exactly which key press opens/advances/closes. A chest
    // just toggles on E; dialogue advances through nodes on E and also needs Escape, plus a
    // BeforeOpen event and a "don't open with no tree assigned" guard. Those differences stay in
    // each subclass's own Open()/Update(), which calls BecomeActive() at the point the original code
    // set its own ActiveXyz = this.
    public abstract class RangeInteractable<T> : MonoBehaviour where T : RangeInteractable<T>
    {
        public static T Active { get; private set; }

        protected bool PlayerInRange { get; private set; }
        public bool IsOpen { get; private set; }

        protected virtual void OnDisable()
        {
            if (Active == this)
                Close();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
                PlayerInRange = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null)
                return;

            PlayerInRange = false;
            if (Active == this)
                Close();
        }

        // Closes whichever other instance of T was active and marks this one active instead.
        // Subclasses call this once they've decided they're actually opening.
        protected void BecomeActive()
        {
            if (Active != null && Active != this)
                Active.Close();

            IsOpen = true;
            Active = (T)this;
        }

        public virtual void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            if (Active == this)
                Active = null;
        }
    }
}
