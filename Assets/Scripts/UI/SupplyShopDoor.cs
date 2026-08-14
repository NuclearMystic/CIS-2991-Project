using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    /// <summary>
    /// Shows a Buy prompt when the player walks through this trigger, then opens the linked
    /// <see cref="SupplyShop"/> after the prompt is clicked. Place it over a building doorway
    /// and size its BoxCollider2D to match the door art.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class SupplyShopDoor : MonoBehaviour
    {
        [SerializeField] private SupplyShop shop;

        [Header("Optional prompt art")]
        [Tooltip("Shown on the doorway Buy button. Leave empty to use Unity's standard GUI button.")]
        [SerializeField] private Texture2D buyButtonGraphic;

        private PlayerInventory playerAtDoor;
        private GUIStyle buyLabelStyle;

        private void Reset()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            shop = GetComponent<SupplyShop>();
        }

        private void Awake()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;

            if (shop == null)
            {
                shop = GetComponent<SupplyShop>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (shop == null)
            {
                return;
            }

            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerAtDoor = playerInventory;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var playerInventory = other.GetComponentInParent<PlayerInventory>();
            if (playerInventory != null && playerInventory == playerAtDoor)
            {
                playerAtDoor = null;
            }
        }

        private void OnGUI()
        {
            if (playerAtDoor == null || shop == null || SupplyShop.IsAnyShopOpen)
            {
                return;
            }

            const float panelWidth = 210f;
            const float panelHeight = 88f;
            var panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                Screen.height - panelHeight - 36f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, "Supplies");
            var buyRect = new Rect(panelRect.x + 18f, panelRect.y + 38f, panelRect.width - 36f, 36f);
            if (DrawBuyButton(buyRect))
            {
                shop.Open(playerAtDoor);
            }
        }

        private bool DrawBuyButton(Rect rect)
        {
            var clicked = GUI.Button(rect, buyButtonGraphic == null ? "BUY" : string.Empty);
            if (buyButtonGraphic != null)
            {
                GUI.DrawTexture(rect, buyButtonGraphic, ScaleMode.ScaleToFit);
                GUI.Label(rect, "BUY", BuyLabelStyle);
            }

            return clicked;
        }

        private GUIStyle BuyLabelStyle
        {
            get
            {
                if (buyLabelStyle == null)
                {
                    buyLabelStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                }

                return buyLabelStyle;
            }
        }
    }
}
