using UnityEngine;
using CIS2991Project.Items;

namespace CIS2991Project.Levels
{
    // Placeholder raider base layout, generated in code until the scene gets hand-placed scenery.
    public sealed class RaiderBaseSceneLayout : MonoBehaviour
    {
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite container;
        [SerializeField] private Sprite barrel;
        [SerializeField] private Sprite ammoCrate;
        [SerializeField] private Sprite pistol;
        [SerializeField] private Sprite bandage;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private global::Item bandageItem;
        [SerializeField] private global::Item ammoItem;
        [SerializeField] private global::Item pistolItem;
        [SerializeField] private Sprite tire;
        [SerializeField] private Sprite vendingMachine;
        [SerializeField] private Sprite debris;
        [SerializeField] private Sprite truck;
        [Header("PostApocalypse Tiles")]
        [SerializeField] private Sprite backgroundTileSet;
        [SerializeField] private Sprite buildingTileSet;

        private static readonly Vector2[] CampCenters =
        {
            new Vector2(-4.6f, 4.7f),
            new Vector2(2.3f, 5.5f),
            new Vector2(8.8f, 5.0f),
            new Vector2(1.4f, -5.2f),
            new Vector2(9.3f, -5.1f)
        };

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            TiledGround();
            Block("Approach Road", new Vector2(-10f, 0f), new Vector2(9f, 3f), new Color(.18f, .18f, .17f, .38f), -19);
            Block("Raiders Yard", new Vector2(4f, 0f), new Vector2(19f, 16f), new Color(.29f, .27f, .21f, .30f), -19);
            Fence();
            Prop("Wrecked Truck", truck, new Vector2(-8.5f, -5.4f), 2);
            Prop("Abandoned Vending Machine", vendingMachine, new Vector2(-7.2f, 5.7f), 2);
            Prop("Tire Pile", tire, new Vector2(-9.4f, 4.3f), 2);
            Prop("Street Debris A", debris, new Vector2(-3.3f, -1.7f), 2);
            Prop("Street Debris B", debris, new Vector2(5.8f, 1.3f), 2);
            Camp("Camp 1", CampCenters[0], 1);
            Camp("Camp 2", CampCenters[1], 2);
            Camp("Camp 3", CampCenters[2], 3);
            Camp("Camp 4", CampCenters[3], 4);
            Camp("Camp 5", CampCenters[4], 5);
            TileSetFeature("Damaged Building A", new Vector2(-8.5f, 5.5f), new Vector2(2.1f, 2.1f));
            TileSetFeature("Damaged Building B", new Vector2(10.8f, 3.6f), new Vector2(1.7f, 1.7f));
            Anchor("RaiderBaseSpawn", new Vector2(-12.2f, 0f));
            Anchor("RaiderBaseExit", new Vector2(13.8f, -7f));
            Exit("Overworld Exit", new Vector2(-12.4f, -2.1f), "Overworld");
        }

        private void Start()
        {
            var player = Object.FindAnyObjectByType<CIS2991Project.Player.PlayerHealth>();
            if (player != null && playerSprite != null)
                player.GetComponent<SpriteRenderer>().sprite = playerSprite;

            if (player != null)
            {
                GiveStarterLoadout(player.GetComponent<CIS2991Project.Player.PlayerInventory>());
            }
        }

        private void GiveStarterLoadout(CIS2991Project.Player.PlayerInventory inventory)
        {
            if (inventory == null)
                return;

            if (pistolItem != null && !inventory.HasItem(pistolItem))
                inventory.TryAdd(pistolItem);
            if (ammoItem != null && !inventory.HasItem(ammoItem))
                inventory.TryAdd(ammoItem, 18);
            if (bandageItem != null && !inventory.HasItem(bandageItem))
                inventory.TryAdd(bandageItem, 2);

            inventory.TryEquip(pistolItem);
        }

        private void Fence()
        {
            for (var x = -5f; x <= 14f; x += 1.1f)
            {
                Prop("Perimeter Wall", wall, new Vector2(x, 8.1f), 1);
                Prop("Perimeter Wall", wall, new Vector2(x, -8.1f), 1);
            }
            for (var y = -7f; y <= 7f; y += 1.1f)
                Prop("Perimeter Wall", wall, new Vector2(14.1f, y), 1);
        }

        private void TiledGround()
        {
            if (backgroundTileSet == null)
            {
                Block("Raiders Ground", Vector2.zero, new Vector2(30f, 20f), new Color(.25f, .25f, .20f), -20);
                return;
            }

            var ground = new GameObject("PostApocalypse Green Tile Ground");
            var renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sprite = backgroundTileSet;
            renderer.sortingOrder = -21;
            ground.transform.localScale = new Vector3(7.82f, 7.36f, 1f);
        }

        private void TileSetFeature(string name, Vector2 position, Vector2 scale)
        {
            if (buildingTileSet == null)
                return;

            var feature = new GameObject(name);
            feature.transform.position = position;
            feature.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var renderer = feature.AddComponent<SpriteRenderer>();
            renderer.sprite = buildingTileSet;
            renderer.sortingOrder = -4;
        }

        private void Camp(string name, Vector2 center, int tier)
        {
            Block(name + " Zone", center, new Vector2(5.3f, 4.1f), new Color(.34f, .29f, .22f, .42f), -5);
            Prop(name + " Barricade", container, center + new Vector2(0f, 1.35f), 1);
            Prop(name + " Fuel", barrel, center + new Vector2(-1.6f, -.9f), 2);
            Prop(name + " Stash", ammoCrate, center + new Vector2(1.55f, -.9f), 3);
            if (tier >= 3) Prop(name + " Weapon", pistol, center + new Vector2(.5f, -.75f), 4);
            if (tier == 5) Prop(name + " Medical Cache", bandage, center + new Vector2(-.5f, -.75f), 4);
            Pickup(name + " Ammo Pickup", ammoCrate, ammoItem, center + new Vector2(1.55f, -.9f), tier + 1);
            if (tier >= 3) Pickup(name + " Pistol Pickup", pistol, pistolItem, center + new Vector2(.5f, -.75f), 1);
            if (tier == 5) Pickup(name + " Bandage Pickup", bandage, bandageItem, center + new Vector2(-.5f, -.75f), 2);
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

        private static void Exit(string name, Vector2 position, string destination)
        {
            var exit = new GameObject(name);
            exit.transform.position = position;
            exit.AddComponent<CircleCollider2D>().radius = 1.2f;
            exit.AddComponent<TeleportPoint>().Configure(destination);
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
