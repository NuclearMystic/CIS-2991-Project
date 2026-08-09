using UnityEngine;
using CIS2991Project.Enemies;
using CIS2991Project.Items;

namespace CIS2991Project.Levels
{
    /// <summary>
    /// Five zombie encounters which unlock one at a time. Each level requires five more
    /// kills than the previous one, giving the player a gentle ramp into the final wave.
    /// </summary>
    public sealed class RaiderBaseSceneLayout : MonoBehaviour
    {
        [SerializeField] private Sprite wall;
        [SerializeField] private Sprite container;
        [SerializeField] private Sprite barrel;
        [SerializeField] private Sprite ammoCrate;
        [SerializeField] private Sprite pistol;
        [SerializeField] private Sprite bandage;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private Sprite raiderSprite;
        [SerializeField] private Sprite bulletSprite;
        [SerializeField] private global::Item bandageItem;
        [SerializeField] private global::Item ammoItem;
        [SerializeField] private global::Item pistolItem;
        [SerializeField] private Sprite tire;
        [SerializeField] private Sprite vendingMachine;
        [SerializeField] private Sprite debris;
        [SerializeField] private Sprite truck;
        [Header("PostApocalypse Tiles and Combat Sheets")]
        [SerializeField] private Sprite backgroundTileSet;
        [SerializeField] private Sprite buildingTileSet;
        [SerializeField] private Sprite zombieAttackSheet;
        [SerializeField] private Sprite enemyShotOneSheet;
        [SerializeField] private Sprite enemyShotTwoSheet;
        [SerializeField] private Sprite gunIdleSheet;
        [SerializeField] private Sprite gunLeftIdleSheet;
        [SerializeField] private Sprite gunShootSheet;
        [SerializeField] private Sprite gunDownShootSheet;
        [SerializeField] private Sprite shotgunIdleSheet;
        [SerializeField] private Sprite pistolReloadSheet;
        [SerializeField] private Sprite shotgunReloadSheet;
        [SerializeField] private Sprite gunDeathSheet;
        [SerializeField] private Sprite gunLeftDeathSheet;

        private const int LevelCount = 5;
        private const int FirstLevelKills = 5;
        private const int KillIncreasePerLevel = 5;
        private const float MinimumSpawnDistanceFromPlayer = 4f;
        private const float SpawnRadius = 2.25f;

        private static readonly Vector2[] LevelCenters =
        {
            new Vector2(-4.6f, 4.7f),
            new Vector2(2.3f, 5.5f),
            new Vector2(8.8f, 5.0f),
            new Vector2(1.4f, -5.2f),
            new Vector2(9.3f, -5.1f)
        };

        public int CurrentLevel { get; private set; }
        public int KillsThisLevel { get; private set; }
        public int KillsRequiredThisLevel => CurrentLevel == 0 ? 0 : KillsForLevel(CurrentLevel);
        public bool IsComplete { get; private set; }
        private bool _awaitingCharacterChoice;
        private CIS2991Project.Player.PlayerSheetVisuals _playerVisuals;

        private void Awake()
        {
            Build();
            DemoEnemy.ZombieKilled += RegisterZombieKill;
        }

        private void OnDestroy()
        {
            DemoEnemy.ZombieKilled -= RegisterZombieKill;
        }

        private void Build()
        {
            EnsureCamera();
            TiledGround();
            Block("Approach Road", new Vector2(-10f, 0f), new Vector2(9f, 3f), new Color(.18f, .18f, .17f, .38f), -19);
            Block("Raiders Yard", new Vector2(4f, 0f), new Vector2(19f, 16f), new Color(.29f, .27f, .21f, .30f), -19);
            Fence();
            Prop("Wrecked Truck", truck, new Vector2(-8.5f, -5.4f), 2);
            Prop("Abandoned Vending Machine", vendingMachine, new Vector2(-7.2f, 5.7f), 2);
            Prop("Tire Pile", tire, new Vector2(-9.4f, 4.3f), 2);
            Prop("Street Debris A", debris, new Vector2(-3.3f, -1.7f), 2);
            Prop("Street Debris B", debris, new Vector2(5.8f, 1.3f), 2);
            Camp("Level 1 - Lookouts", LevelCenters[0], 1, KillsForLevel(1), "slow patrols");
            Camp("Level 2 - Wreckers", LevelCenters[1], 2, KillsForLevel(2), "mixed patrols");
            Camp("Level 3 - Garage", LevelCenters[2], 3, KillsForLevel(3), "crossfire lanes");
            Camp("Level 4 - Barracks", LevelCenters[3], 4, KillsForLevel(4), "fast response");
            Camp("Level 5 - Boss Stash", LevelCenters[4], 5, KillsForLevel(5), "high pressure finale");
            TileSetFeature("Damaged Building A", new Vector2(-8.5f, 5.5f), new Vector2(2.1f, 2.1f));
            TileSetFeature("Damaged Building B", new Vector2(10.8f, 3.6f), new Vector2(1.7f, 1.7f));
            Anchor("RaiderBaseSpawn", new Vector2(-12.2f, 0f));
            Anchor("RaiderBaseExit", new Vector2(13.8f, -7f));
            Portal("Settlement Return Gate", new Vector2(-12.4f, -2.1f), "Settlement", "Press E to return to settlement");
        }

        private void Start()
        {
            var player = Object.FindAnyObjectByType<CIS2991Project.Player.PlayerHealth>();
            if (player != null && playerSprite != null)
                player.GetComponent<SpriteRenderer>().sprite = playerSprite;

            if (player != null)
            {
                var shooter = player.GetComponent<CIS2991Project.Player.PlayerShoot>();
                if (shooter != null)
                    shooter.ConfigureProjectileVisual(bulletSprite);

                _playerVisuals = player.GetComponent<CIS2991Project.Player.PlayerSheetVisuals>();
                if (_playerVisuals == null)
                    _playerVisuals = player.gameObject.AddComponent<CIS2991Project.Player.PlayerSheetVisuals>();
                _playerVisuals.Configure(gunIdleSheet, gunLeftIdleSheet, gunShootSheet, gunDownShootSheet,
                    shotgunIdleSheet, pistolReloadSheet, shotgunReloadSheet, gunDeathSheet, gunLeftDeathSheet);

                GiveStarterLoadout(player.GetComponent<CIS2991Project.Player.PlayerInventory>());
            }

            _awaitingCharacterChoice = true;
        }

        private void Update()
        {
            if (_awaitingCharacterChoice || IsComplete || CurrentLevel == 0 || KillsThisLevel < KillsRequiredThisLevel || DemoEnemy.ActiveZombieCount > 0)
                return;

            if (CurrentLevel == LevelCount)
            {
                IsComplete = true;
                return;
            }

            StartLevel(CurrentLevel + 1);
        }

        private void RegisterZombieKill()
        {
            if (!IsComplete && CurrentLevel > 0)
                KillsThisLevel = Mathf.Min(KillsThisLevel + 1, KillsRequiredThisLevel);
        }

        private void StartLevel(int level)
        {
            CurrentLevel = level;
            KillsThisLevel = 0;
            SpawnEncounter(LevelCenters[level - 1], level, KillsForLevel(level), .65f + level * .15f);
        }

        private void OnGUI()
        {
            if (!_awaitingCharacterChoice)
                return;

            const float width = 460f;
            const float height = 210f;
            var x = (Screen.width - width) * .5f;
            var y = (Screen.height - height) * .5f;
            GUI.Box(new Rect(x, y, width, height), "Choose your survivor");
            GUI.Label(new Rect(x + 28f, y + 40f, width - 56f, 42f),
                "Pick a combat style. Your choice changes the character animation and starting stance.");
            if (GUI.Button(new Rect(x + 28f, y + 105f, 185f, 62f), "1  Gunner\nPistol stance"))
                ChooseCharacter(false);
            if (GUI.Button(new Rect(x + 247f, y + 105f, 185f, 62f), "2  Scout\nShotgun stance"))
                ChooseCharacter(true);
        }

        private void ChooseCharacter(bool shotgunStance)
        {
            _playerVisuals?.SelectShotgunStance(shotgunStance);
            _awaitingCharacterChoice = false;
            StartLevel(1);
        }

        private void SpawnEncounter(Vector2 center, int tier, int count, float speed)
        {
            var drops = tier >= 4
                ? new[] { bandageItem, ammoItem, pistolItem }
                : new[] { bandageItem, ammoItem };
            for (var index = 0; index < count; index++)
            {
                var spawnPosition = RandomSpawnPosition(center);
                var patrolDirection = Random.insideUnitCircle.normalized;
                if (patrolDirection == Vector2.zero)
                    patrolDirection = Vector2.right;
                DemoEnemy.Spawn(spawnPosition, spawnPosition + patrolDirection * 1.2f, raiderSprite, ammoCrate, drops, tier + 1, speed,
                    zombieAttackSheet, enemyShotOneSheet, enemyShotTwoSheet);
            }
        }

        private static Vector2 RandomSpawnPosition(Vector2 center)
        {
            var player = Object.FindAnyObjectByType<CIS2991Project.Player.PlayerHealth>();
            for (var attempt = 0; attempt < 24; attempt++)
            {
                var candidate = center + Random.insideUnitCircle * SpawnRadius;
                if (player == null || Vector2.Distance(candidate, player.transform.position) >= MinimumSpawnDistanceFromPlayer)
                    return candidate;
            }

            var safeDirection = player == null
                ? Vector2.up
                : (center - (Vector2)player.transform.position).normalized;
            return center + safeDirection * SpawnRadius;
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

        private static int KillsForLevel(int level)
        {
            return FirstLevelKills + (level - 1) * KillIncreasePerLevel;
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

        private void Camp(string name, Vector2 center, int tier, int enemies, string pacing)
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
            Anchor($"RaiderTier{tier}_Spawn_{pacing.Replace(" ", "_")}", center + new Vector2(0f, .15f));
            Anchor($"RaiderTier{tier}_EnemyCount_{enemies}", center + new Vector2(-1f, .2f));
            Anchor($"RaiderTier{tier}_LootAmmo", center + new Vector2(1.55f, -.9f));
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = 10.5f;
                Camera.main.backgroundColor = new Color(.12f, .13f, .12f);
                return;
            }
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
