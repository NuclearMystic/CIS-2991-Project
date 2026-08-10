using UnityEngine;
using CIS2991Project.Items;

namespace CIS2991Project.Levels
{
    /// <summary>
    /// Visual-only layout for the friendly hub. Gameplay systems can attach to the named
    /// anchors (SettlementSpawn, ShopCounter, and TownLoot_*) without editing this scene.
    /// </summary>
    public sealed class SettlementSceneLayout : MonoBehaviour
    {
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite entrance;
        [SerializeField] private Sprite boardedDoor;
        [SerializeField] private Sprite cart;
        [SerializeField] private Sprite barrel;
        [SerializeField] private Sprite bandage;
        [SerializeField] private Sprite ammo;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private global::Item bandageItem;
        [SerializeField] private global::Item ammoItem;

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            EnsureCamera(new Color(0.20f, 0.24f, 0.20f));
            Block("Settlement Ground", Vector2.zero, new Vector2(28f, 19f), new Color(0.29f, 0.34f, 0.25f), -20);
            Block("Main Street", new Vector2(0f, -1.8f), new Vector2(28f, 3.1f), new Color(0.23f, 0.23f, 0.21f), -19);
            Block("Market Square", new Vector2(-1f, 3.9f), new Vector2(8f, 3.5f), new Color(0.32f, 0.30f, 0.24f), -18);

            House("Settler House A", new Vector2(-8.7f, 5.8f), new Vector2(5.2f, 3.7f), false);
            House("Settler House B", new Vector2(-1.8f, 6.5f), new Vector2(4.5f, 3.2f), true);
            House("Settler House C", new Vector2(7.8f, 5.9f), new Vector2(5.8f, 3.9f), false);
            Shop("Salvage & Supply", new Vector2(7.6f, -5.3f));
            Shop("Clinic", new Vector2(-7.4f, -5.4f));

            Prop("Shopping Cart", cart, new Vector2(0.2f, 2.4f), 2);
            Prop("Barrel Fire", barrel, new Vector2(-2.4f, 2.3f), 2);
            Prop("Bandage Display", bandage, new Vector2(-7.2f, -4.7f), 5);
            Prop("Ammo Display", ammo, new Vector2(7.7f, -4.6f), 5);
            Pickup("Clinic Bandage", bandage, bandageItem, new Vector2(-5.8f, -5.6f), 2);
            Pickup("Supply Ammo", ammo, ammoItem, new Vector2(5.8f, -5.6f), 6);
            Anchor("SettlementSpawn", new Vector2(0f, -0.3f));
            Anchor("ShopCounter_Salvage", new Vector2(7.6f, -4.7f));
            Anchor("ShopCounter_Clinic", new Vector2(-7.4f, -4.7f));
            Anchor("TownLoot_Bandage", new Vector2(-5.8f, -5.6f));
            Anchor("TownLoot_Ammo", new Vector2(5.8f, -5.6f));
            Portal("Raider Base Gate", new Vector2(12.5f, -1.8f), "RaiderBase", "Press E to enter the raider base");
        }

        private void Start()
        {
            var player = Object.FindAnyObjectByType<CIS2991Project.Player.PlayerHealth>();
            if (player != null && playerSprite != null)
                player.GetComponent<SpriteRenderer>().sprite = playerSprite;
        }

        private void House(string name, Vector2 center, Vector2 size, bool boarded)
        {
            Block(name + " Interior", center, size, new Color(0.40f, 0.37f, 0.29f), -5);
            var half = size * 0.5f;
            for (var x = center.x - half.x + .55f; x <= center.x + half.x - .55f; x += 1.1f)
            {
                Prop(name + " Roof Wall", wall, new Vector2(x, center.y + half.y), 1);
                Prop(name + " Foundation", wall, new Vector2(x, center.y - half.y), 1);
            }
            Prop(name + " Door", boarded ? boardedDoor : entrance, new Vector2(center.x, center.y - half.y + .35f), 3);
        }

        private void Shop(string name, Vector2 center)
        {
            Block(name + " Interior", center, new Vector2(6.2f, 3.5f), new Color(0.35f, 0.33f, 0.27f), -5);
            for (var x = center.x - 2.7f; x <= center.x + 2.7f; x += 1.08f)
                Prop(name + " Facade", wall, new Vector2(x, center.y + 1.72f), 1);
            Prop(name + " Entrance", entrance, new Vector2(center.x, center.y - 1.35f), 3);
            Prop(name + " Stock", barrel, new Vector2(center.x + 2.25f, center.y - .8f), 2);
        }

        private static void EnsureCamera(Color background)
        {
            if (Camera.main != null) return;
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10.5f;
            camera.backgroundColor = background;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
        }

        private static void Block(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = Solid(color);
            renderer.sortingOrder = order;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static void Anchor(string name, Vector2 position)
        {
            var anchor = new GameObject(name);
            anchor.transform.position = position;
        }

        private static void Portal(string name, Vector2 position, string destination, string prompt)
        {
            var portal = new GameObject(name);
            portal.transform.position = position;
            portal.AddComponent<CircleCollider2D>().radius = 1.2f;
            portal.AddComponent<ScenePortal>().Configure(destination, prompt);
        }

        private static void Pickup(string name, Sprite sprite, global::Item item, Vector2 position, int amount)
        {
            if (sprite == null || item == null) return;
            var pickup = new GameObject(name);
            pickup.transform.position = position;
            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;
            pickup.AddComponent<CircleCollider2D>().isTrigger = true;
            pickup.AddComponent<ConsumablePickup>().Configure(item, amount);
        }

        private static void Prop(string name, Sprite sprite, Vector2 position, int order)
        {
            if (sprite == null) return;
            var prop = new GameObject(name);
            prop.transform.position = position;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
        }

        private static Sprite Solid(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        }
    }
}
