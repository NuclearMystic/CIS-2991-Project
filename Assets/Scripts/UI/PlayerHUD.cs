using CIS2991Project.Items;
using CIS2991Project.Jobs;
using CIS2991Project.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    // Composes the HUD from focused panel classes (VitalsHud, InventoryPanel, SkillsPanel, etc). All
    // Inspector-wired art/layout fields stay here rather than moving onto the panels, so the Player
    // prefab's existing Inspector wiring never has to change - the panels just receive the values
    // this component already holds when they're constructed in Awake.
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerShoot playerShoot;
        [SerializeField] private CharacterSheet characterSheet;
        [SerializeField] private SurvivalStats survivalStats;
        [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;
        [SerializeField] private KeyCode skillsToggleKey = KeyCode.K;
        [SerializeField] private KeyCode journalToggleKey = KeyCode.J;
        [SerializeField] private KeyCode allMenusToggleKey = KeyCode.Tab;

        [Header("Hearts (Zelda-style) — CurrentHealth/MaxHealth is mapped onto this many hearts")]
        [SerializeField] private Texture2D fullHeartTexture;
        [SerializeField] private Texture2D halfHeartTexture;
        [SerializeField] private Texture2D emptyHeartTexture;
        [Tooltip("Fallback heart count used only if no CharacterSheet is found (Endurance skill normally drives this).")]
        [SerializeField, Min(1)] private int heartCount = 10;
        [SerializeField, Min(1)] private int heartsPerRow = 10;
        [SerializeField, Min(4f)] private float heartSize = 28f;
        [SerializeField, Min(0f)] private float heartSpacing = 2f;

        [Header("Ammo — icon set switches with the equipped weapon's ammo type; row length follows its magazine size")]
        [SerializeField] private Texture2D pistolAmmoFullTexture;
        [SerializeField] private Texture2D pistolAmmoEmptyTexture;
        [SerializeField] private Texture2D shotgunAmmoFullTexture;
        [SerializeField] private Texture2D shotgunAmmoEmptyTexture;
        [SerializeField] private Texture2D rifleAmmoFullTexture;
        [SerializeField] private Texture2D rifleAmmoEmptyTexture;
        [SerializeField, Min(1)] private int ammoPerRow = 10;
        [SerializeField, Min(4f)] private float ammoIconSize = 20f;
        [SerializeField, Min(0f)] private float ammoIconSpacing = 2f;

        [Header("Reload Bar — floats above the player while reloading, drains as reload progresses")]
        [SerializeField] private Texture2D reloadBarBackgroundTexture;
        [SerializeField] private Texture2D reloadBarFillTexture;
        [SerializeField, Min(4f)] private float reloadBarWidth = 100f;
        [SerializeField, Min(4f)] private float reloadBarHeight = 14f;
        [SerializeField] private float reloadBarWorldOffset = 1.2f;

        [Header("Pickup Popup — floats above the player for a moment when an item is picked up")]
        [SerializeField] private float pickupPopupWorldOffset = 1.6f;
        [SerializeField, Min(0f)] private float pickupPopupRiseDistance = 1f;
        [SerializeField, Min(0.1f)] private float pickupPopupDuration = 2.5f;
        [SerializeField, Min(6)] private int pickupPopupFontSize = 18;

        [Header("Damage Numbers — floats above the player when they take damage or are healed")]
        [SerializeField] private float damageNumberWorldOffset = 1.6f;
        [SerializeField, Min(0f)] private float damageNumberRiseDistance = 1f;
        [SerializeField, Min(0.1f)] private float damageNumberDuration = 1f;
        [SerializeField, Min(6)] private int damageNumberFontSize = 20;
        [Tooltip("Random horizontal spread (screen pixels) so numbers from back-to-back hits don't stack exactly on top of each other.")]
        [SerializeField, Min(0f)] private float damageNumberHorizontalScatter = 18f;

        [Header("Equipment Slots (Weapon / Outfit boxes, bottom-left)")]
        [SerializeField, Min(4f)] private float equipmentBoxSize = 56f;
        [SerializeField, Min(0f)] private float equipmentBoxGap = 8f;

        [Header("Inventory / Bag — assign your own art here")]
        [SerializeField] private Texture2D bagIconTexture;
        [SerializeField] private Texture2D inventoryBorderTexture;
        [SerializeField] private Texture2D inventorySlotBackgroundTexture;
        [SerializeField, Min(4f)] private float bagButtonWidth = 90f;
        [SerializeField, Min(4f)] private float bagButtonHeight = 50f;
        [SerializeField, Min(4f)] private float inventoryPanelWidth = 430f;
        [SerializeField, Min(4f)] private float inventoryPanelHeight = 360f;
        [SerializeField, Min(4f)] private float inventorySlotWidth = 78f;
        [SerializeField, Min(4f)] private float inventorySlotHeight = 36f;
        [SerializeField, Min(0f)] private float inventorySlotGap = 4f;
        [SerializeField, Min(0f)] private float inventoryGridTopPadding = 44f;

        [Header("Assign your own art here — slots render as plain boxes until then")]
        [SerializeField] private Texture2D moneyTexture;
        [SerializeField] private string currencyName = "Caps";
        [SerializeField, Min(4f)] private float moneyWidth = 190f;
        [SerializeField, Min(4f)] private float moneyHeight = 44f;
        [SerializeField, Min(6)] private int moneyFontSize = 26;
        [SerializeField] private Texture2D[] hotbarSlotTextures = new Texture2D[PlayerInventory.HotbarSlotCount];
        [SerializeField, Min(4f)] private float hotbarSlotSize = 64f;
        [SerializeField, Min(0f)] private float hotbarSlotGap = 8f;

        [Header("Skills — allocate points earned from leveling up")]
        [SerializeField] private Texture2D skillsPanelBackgroundTexture;
        [SerializeField, Min(4f)] private float skillsPanelWidth = 320f;
        [SerializeField, Min(4f)] private float skillRowHeight = 26f;

        [Header("Job Journal — Active/Completed jobs, opened with Journal toggle key")]
        [SerializeField] private Texture2D journalPanelBackgroundTexture;
        [SerializeField, Min(4f)] private float journalPanelWidth = 460f;
        [SerializeField, Min(4f)] private float journalPanelHeight = 340f;
        [SerializeField, Min(4f)] private float journalRowHeight = 46f;

        [Header("Job Tracker — always-visible kill-count readout for active jobs")]
        [SerializeField] private Texture2D jobTrackerRowBackgroundTexture;
        [SerializeField, Min(4f)] private float jobTrackerWidth = 220f;
        [SerializeField, Min(4f)] private float jobTrackerRowHeight = 26f;
        [SerializeField, Min(0f)] private float jobTrackerRowGap = 2f;

        [Header("Survival — Hunger/Thirst/Radiation meters and vision haze. Fill color is used unless " +
                "you assign your own fill art, which takes priority.")]
        [SerializeField] private Texture2D hungerBarBackgroundTexture;
        [SerializeField] private Texture2D hungerBarFillTexture;
        [SerializeField] private Color hungerBarFillColor = new(0.1f, 0.35f, 0.1f);
        [SerializeField] private Texture2D thirstBarBackgroundTexture;
        [SerializeField] private Texture2D thirstBarFillTexture;
        [SerializeField] private Color thirstBarFillColor = new(0.15f, 0.4f, 0.85f);
        [Tooltip("Placeholder only - radiation isn't wired to any real system yet, this just displays radiationPlaceholderValue.")]
        [SerializeField] private Texture2D radiationBarBackgroundTexture;
        [SerializeField] private Texture2D radiationBarFillTexture;
        [SerializeField] private Color radiationBarFillColor = new(0.8f, 0.15f, 0.15f);
        [SerializeField, Range(0f, 100f)] private float radiationPlaceholderValue = 10f;
        [SerializeField, Min(4f)] private float survivalBarWidth = 160f;
        [SerializeField, Min(4f)] private float survivalBarHeight = 18f;
        [SerializeField, Min(0f)] private float survivalBarGap = 4f;
        [Tooltip("Tint of the full-screen haze overlay that intensifies as hunger/thirst drop.")]
        [SerializeField] private Color survivalHazeColor = new(0.6f, 0.75f, 0.5f);
        [Tooltip("Haze opacity at maximum severity (both meters empty). 0 disables the overlay entirely.")]
        [SerializeField, Range(0f, 1f)] private float survivalMaxHazeAlpha = 0.35f;

        [Header("Empty-Meter Pulse — flashes red on Hunger/Thirst once that bar hits 0")]
        [SerializeField] private Color emptyMeterPulseColor = new(1f, 0.15f, 0.15f);
        [Tooltip("Higher = faster flashing.")]
        [SerializeField, Min(0.1f)] private float emptyMeterPulseSpeed = 4f;
        [SerializeField, Range(0f, 1f)] private float emptyMeterPulseMaxAlpha = 0.6f;

        private bool inventoryVisible;
        private bool skillsVisible;
        private bool journalVisible;

        private DragState _dragState;
        private DoubleClickTracker _doubleClickTracker;
        private VitalsHud _vitalsHud;
        private PickupPopupHud _pickupPopupHud;
        private EquipmentHud _equipmentHud;
        private HotbarHud _hotbarHud;
        private InventoryPanel _inventoryPanel;
        private SkillsPanel _skillsPanel;
        private JournalPanel _journalPanel;
        private JobTrackerHud _jobTrackerHud;
        private SurvivalHud _survivalHud;
        private DamageNumberHud _damageNumberHud;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
            }

            if (playerInventory == null)
            {
                playerInventory = GetComponentInParent<PlayerInventory>();
            }

            if (playerInventory == null)
            {
                playerInventory = Object.FindAnyObjectByType<PlayerInventory>();
            }

            if (playerShoot == null)
            {
                playerShoot = GetComponentInParent<PlayerShoot>();
            }

            if (playerShoot == null)
            {
                playerShoot = Object.FindAnyObjectByType<PlayerShoot>();
            }

            if (characterSheet == null)
            {
                characterSheet = GetComponentInParent<CharacterSheet>();
            }

            if (characterSheet == null)
            {
                characterSheet = Object.FindAnyObjectByType<CharacterSheet>();
            }

            if (survivalStats == null)
            {
                survivalStats = GetComponentInParent<SurvivalStats>();
            }

            if (survivalStats == null)
            {
                survivalStats = Object.FindAnyObjectByType<SurvivalStats>();
            }

            if (playerInventory != null)
            {
                playerInventory.ItemPickedUp += HandleItemPickedUp;
            }

            if (playerHealth != null)
            {
                playerHealth.DamageTaken += HandleDamageTaken;
                playerHealth.Healed += HandleHealed;
            }

            BuildPanels();
        }

        private void BuildPanels()
        {
            _dragState = new DragState();
            _doubleClickTracker = new DoubleClickTracker();

            _vitalsHud = new VitalsHud(
                fullHeartTexture: fullHeartTexture,
                halfHeartTexture: halfHeartTexture,
                emptyHeartTexture: emptyHeartTexture,
                fallbackHeartCount: heartCount,
                heartsPerRow: heartsPerRow,
                heartSize: heartSize,
                heartSpacing: heartSpacing,
                pistolAmmoFullTexture: pistolAmmoFullTexture,
                pistolAmmoEmptyTexture: pistolAmmoEmptyTexture,
                shotgunAmmoFullTexture: shotgunAmmoFullTexture,
                shotgunAmmoEmptyTexture: shotgunAmmoEmptyTexture,
                rifleAmmoFullTexture: rifleAmmoFullTexture,
                rifleAmmoEmptyTexture: rifleAmmoEmptyTexture,
                ammoPerRow: ammoPerRow,
                ammoIconSize: ammoIconSize,
                ammoIconSpacing: ammoIconSpacing,
                reloadBarBackgroundTexture: reloadBarBackgroundTexture,
                reloadBarFillTexture: reloadBarFillTexture,
                reloadBarWidth: reloadBarWidth,
                reloadBarHeight: reloadBarHeight,
                reloadBarWorldOffset: reloadBarWorldOffset);

            _pickupPopupHud = new PickupPopupHud(
                worldOffset: pickupPopupWorldOffset,
                riseDistance: pickupPopupRiseDistance,
                duration: pickupPopupDuration,
                fontSize: pickupPopupFontSize);

            _damageNumberHud = new DamageNumberHud(
                worldOffset: damageNumberWorldOffset,
                riseDistance: damageNumberRiseDistance,
                duration: damageNumberDuration,
                fontSize: damageNumberFontSize,
                horizontalScatter: damageNumberHorizontalScatter);

            _equipmentHud = new EquipmentHud(_doubleClickTracker, equipmentBoxSize, equipmentBoxGap);

            _hotbarHud = new HotbarHud(_dragState, hotbarSlotTextures, hotbarSlotSize, hotbarSlotGap);

            _inventoryPanel = new InventoryPanel(
                dragState: _dragState,
                doubleClickTracker: _doubleClickTracker,
                panelWidth: inventoryPanelWidth,
                panelHeight: inventoryPanelHeight,
                slotWidth: inventorySlotWidth,
                slotHeight: inventorySlotHeight,
                slotGap: inventorySlotGap,
                gridTopPadding: inventoryGridTopPadding,
                borderTexture: inventoryBorderTexture,
                slotBackgroundTexture: inventorySlotBackgroundTexture);

            _skillsPanel = new SkillsPanel(skillsPanelBackgroundTexture, skillsPanelWidth, skillRowHeight);

            _journalPanel = new JournalPanel(journalPanelBackgroundTexture, journalPanelWidth, journalPanelHeight, journalRowHeight);

            _jobTrackerHud = new JobTrackerHud(jobTrackerRowBackgroundTexture, jobTrackerWidth, jobTrackerRowHeight, jobTrackerRowGap, moneyHeight);

            _survivalHud = new SurvivalHud(
                hungerBackgroundTexture: hungerBarBackgroundTexture,
                hungerFillTexture: hungerBarFillTexture,
                hungerFillColor: hungerBarFillColor,
                thirstBackgroundTexture: thirstBarBackgroundTexture,
                thirstFillTexture: thirstBarFillTexture,
                thirstFillColor: thirstBarFillColor,
                radiationBackgroundTexture: radiationBarBackgroundTexture,
                radiationFillTexture: radiationBarFillTexture,
                radiationFillColor: radiationBarFillColor,
                radiationPlaceholderValue: radiationPlaceholderValue,
                barWidth: survivalBarWidth,
                barHeight: survivalBarHeight,
                barGap: survivalBarGap,
                hazeColor: survivalHazeColor,
                maxHazeAlpha: survivalMaxHazeAlpha,
                emptyPulseColor: emptyMeterPulseColor,
                emptyPulseSpeed: emptyMeterPulseSpeed,
                emptyPulseMaxAlpha: emptyMeterPulseMaxAlpha);
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.ItemPickedUp -= HandleItemPickedUp;
            }

            if (playerHealth != null)
            {
                playerHealth.DamageTaken -= HandleDamageTaken;
                playerHealth.Healed -= HandleHealed;
            }
        }

        private void HandleItemPickedUp(global::Item item, int amount)
        {
            _pickupPopupHud.Show(item, amount);
        }

        private void HandleDamageTaken(int amount)
        {
            _damageNumberHud.ShowDamage(amount);
        }

        private void HandleHealed(int amount)
        {
            _damageNumberHud.ShowHeal(amount);
        }

        private void Update()
        {
            if (Input.GetKeyDown(inventoryToggleKey))
            {
                inventoryVisible = !inventoryVisible;
            }

            if (Input.GetKeyDown(skillsToggleKey))
            {
                skillsVisible = !skillsVisible;
            }

            if (Input.GetKeyDown(journalToggleKey))
            {
                journalVisible = !journalVisible;
            }

            if (Input.GetKeyDown(allMenusToggleKey))
            {
                var anyOpen = inventoryVisible || skillsVisible || journalVisible;
                inventoryVisible = !anyOpen;
                skillsVisible = !anyOpen;
                journalVisible = !anyOpen;
            }

            _pickupPopupHud.Tick(Time.deltaTime);
            _damageNumberHud.Tick(Time.deltaTime);
        }

        private void OnGUI()
        {
            GuiScale.Begin();

            var nextY = _vitalsHud.DrawHearts(playerHealth, characterSheet);
            DrawInventoryToggleButton();
            DrawSkillsToggleButton();
            DrawJournalToggleButton();

            nextY = _survivalHud.Draw(survivalStats, 16f, nextY);

            GUI.Label(new Rect(16f, nextY, 520f, 22f), $"{SceneManager.GetActiveScene().name}  |  Move: WASD/Arrows  Shoot: Space  Sprint: Shift  Inventory: I  Skills: K  Journal: J  All: Tab");
            nextY += 22f;

            nextY = _vitalsHud.DrawAmmo(playerShoot, nextY);

            DrawMoneyHud();
            _jobTrackerHud.Draw();
            _hotbarHud.Draw(playerInventory);
            _equipmentHud.Draw(playerInventory, playerShoot);
            _vitalsHud.DrawReloadBar(playerShoot);
            _pickupPopupHud.Draw(playerInventory);
            _damageNumberHud.Draw(playerHealth != null ? playerHealth.transform : null);

            var chestOpen = ChestInventory.ActiveChest != null;

            if (inventoryVisible || chestOpen)
            {
                _inventoryPanel.DrawPlayerInventory(playerInventory, playerHealth, nextY);
            }

            if (chestOpen)
            {
                _inventoryPanel.DrawChestInventory(playerInventory, ChestInventory.ActiveChest, nextY);
            }

            if (skillsVisible)
            {
                _skillsPanel.Draw(characterSheet, nextY);
            }

            if (journalVisible && _journalPanel.Draw())
            {
                journalVisible = false;
            }

            _inventoryPanel.DrawDraggedPreview(playerInventory);

            _survivalHud.DrawHazeOverlay(survivalStats);

            if (Event.current.type == EventType.MouseUp)
            {
                _dragState.SlotIndex = -1;
            }
        }

        private GUIStyle _moneyLabelStyle;

        private GUIStyle MoneyLabelStyle => GuiDrawUtils.GetOrCreate(ref _moneyLabelStyle, () => new GUIStyle(GuiDrawUtils.CenteredLabelStyle)
        {
            fontSize = moneyFontSize,
            fontStyle = FontStyle.Bold
        });

        private void DrawMoneyHud()
        {
            var rect = new Rect(GuiScale.ReferenceWidth - moneyWidth - 16f, 16f, moneyWidth, moneyHeight);
            GuiDrawUtils.DrawSlot(rect, moneyTexture);

            var amount = playerInventory != null ? playerInventory.Currency : 0;
            GUI.Label(rect, $"{amount} {currencyName}", MoneyLabelStyle);
        }

        private void DrawInventoryToggleButton()
        {
            var heartsRowWidth = _vitalsHud.GetHeartsRowWidth(characterSheet);
            var buttonX = 16f + heartsRowWidth + 12f;
            var rect = new Rect(buttonX, 16f, bagButtonWidth, bagButtonHeight);

            var clicked = GUI.Button(rect, bagIconTexture != null ? string.Empty : "Bag");
            if (bagIconTexture != null)
            {
                GUI.DrawTexture(rect, bagIconTexture, ScaleMode.ScaleToFit);
            }

            if (clicked)
            {
                inventoryVisible = !inventoryVisible;
            }
        }

        private void DrawSkillsToggleButton()
        {
            var heartsRowWidth = _vitalsHud.GetHeartsRowWidth(characterSheet);
            var bagX = 16f + heartsRowWidth + 12f;
            var rect = new Rect(bagX + bagButtonWidth + 8f, 16f, bagButtonWidth, bagButtonHeight);

            var label = characterSheet != null && characterSheet.UnspentSkillPoints > 0
                ? $"Skills ({characterSheet.UnspentSkillPoints})"
                : "Skills";

            if (GUI.Button(rect, label))
            {
                skillsVisible = !skillsVisible;
            }
        }

        private void DrawJournalToggleButton()
        {
            var heartsRowWidth = _vitalsHud.GetHeartsRowWidth(characterSheet);
            var bagX = 16f + heartsRowWidth + 12f;
            var rect = new Rect(bagX + (bagButtonWidth + 8f) * 2f, 16f, bagButtonWidth, bagButtonHeight);

            var activeCount = JobManager.ActiveJobs.Count;
            var label = activeCount > 0 ? $"Journal ({activeCount})" : "Journal";

            if (GUI.Button(rect, label))
            {
                journalVisible = !journalVisible;
            }
        }
    }
}
