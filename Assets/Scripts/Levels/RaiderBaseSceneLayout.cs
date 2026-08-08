using UnityEngine;

namespace CIS2991Project.Levels
{
    /// <summary>
    /// Five escalating raider encounter pockets. Named spawn and loot anchors are intentionally
    /// data-only so combat, drops, and level transitions can be wired in later.
    /// </summary>
    public sealed class RaiderBaseSceneLayout : MonoBehaviour
    {
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite container;
        [SerializeField] private Sprite barrel;
        [SerializeField] private Sprite ammoCrate;
        [SerializeField] private Sprite pistol;
        [SerializeField] private Sprite bandage;

        private void Awake() => Build();

        private void Build()
        {
            EnsureCamera();
            Block("Raiders Ground", Vector2.zero, new Vector2(30f, 20f), new Color(.25f, .25f, .20f), -20);
            Block("Approach Road", new Vector2(-10f, 0f), new Vector2(9f, 3f), new Color(.18f, .18f, .17f), -19);
            Block("Raiders Yard", new Vector2(4f, 0f), new Vector2(19f, 16f), new Color(.29f, .27f, .21f), -19);
            Fence();
            Camp("Level 1 - Lookouts", new Vector2(-4.6f, 4.7f), 1, 2, "slow patrols");
            Camp("Level 2 - Wreckers", new Vector2(2.3f, 5.5f), 2, 3, "mixed patrols");
            Camp("Level 3 - Garage", new Vector2(8.8f, 5.0f), 3, 4, "crossfire lanes");
            Camp("Level 4 - Barracks", new Vector2(1.4f, -5.2f), 4, 5, "fast response");
            Camp("Level 5 - Boss Stash", new Vector2(9.3f, -5.1f), 5, 6, "high pressure finale");
            Anchor("RaiderBaseSpawn", new Vector2(-12.2f, 0f));
            Anchor("RaiderBaseExit", new Vector2(13.8f, -7f));
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

        private void Camp(string name, Vector2 center, int tier, int enemies, string pacing)
        {
            Block(name + " Zone", center, new Vector2(5.3f, 4.1f), new Color(.34f, .29f, .22f), -5);
            Prop(name + " Barricade", container, center + new Vector2(0f, 1.35f), 1);
            Prop(name + " Fuel", barrel, center + new Vector2(-1.6f, -.9f), 2);
            Prop(name + " Stash", ammoCrate, center + new Vector2(1.55f, -.9f), 3);
            if (tier >= 3) Prop(name + " Weapon", pistol, center + new Vector2(.5f, -.75f), 4);
            if (tier == 5) Prop(name + " Medical Cache", bandage, center + new Vector2(-.5f, -.75f), 4);
            Anchor($"RaiderTier{tier}_Spawn_{pacing.Replace(" ", "_")}", center + new Vector2(0f, .15f));
            Anchor($"RaiderTier{tier}_EnemyCount_{enemies}", center + new Vector2(-1f, .2f));
            Anchor($"RaiderTier{tier}_LootAmmo", center + new Vector2(1.55f, -.9f));
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10.5f;
            camera.backgroundColor = new Color(.12f, .13f, .12f);
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
