using CIS2991Project.Items;
using CIS2991Project.Jobs;
using CIS2991Project.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        private const int InventoryColumns = 5;

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerShoot playerShoot;
        [SerializeField] private CharacterSheet characterSheet;
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
        [SerializeField, Min(4f)] private float moneyWidth = 150f;
        [SerializeField, Min(4f)] private float moneyHeight = 32f;
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

        private const double DoubleClickSeconds = 0.3;
        private const int WeaponBoxClickId = -100;
        private const int OutfitBoxClickId = -200;

        private bool inventoryVisible;
        private bool skillsVisible;
        private bool journalVisible;
        private int journalTab;
        private int selectedInventorySlot = -1;
        private int _lastClickId = int.MinValue;
        private double _lastClickTime = -1d;
        private GUIStyle _centeredLabelStyle;
        private GUIStyle _pickupPopupStyle;
        private int _draggedInventorySlot = -1;
        private string _pickupPopupText;
        private float _pickupPopupTimeRemaining;

        // Plain GUI.Label has no background box, unlike GUI.Button, so it's safe to draw
        // on top of a slot's background texture without covering it back up.
        private GUIStyle CenteredLabelStyle
        {
            get
            {
                if (_centeredLabelStyle == null)
                {
                    _centeredLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                }

                return _centeredLabelStyle;
            }
        }

        private GUIStyle PickupPopupStyle
        {
            get
            {
                if (_pickupPopupStyle == null)
                {
                    _pickupPopupStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = pickupPopupFontSize
                    };
                    _pickupPopupStyle.normal.textColor = Color.green;
                }

                return _pickupPopupStyle;
            }
        }

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

            EnsureHud();

            if (playerInventory != null)
            {
                playerInventory.ItemPickedUp += HandleItemPickedUp;
            }
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.ItemPickedUp -= HandleItemPickedUp;
            }
        }

        private int HeartCount => characterSheet != null ? characterSheet.GetHeartCount() : heartCount;

        private void EnsureHud()
        {
            // IMGUI HUD needs no scene setup; OnGUI draws it every frame.
        }

        private void HandleItemPickedUp(global::Item item, int amount)
        {
            var itemName = GetItemName(item).ToUpperInvariant();
            _pickupPopupText = $"{itemName} X{amount}";
            _pickupPopupTimeRemaining = pickupPopupDuration;
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

            if (_pickupPopupTimeRemaining > 0f)
            {
                _pickupPopupTimeRemaining -= Time.deltaTime;
            }
        }

        private void OnGUI()
        {
            GuiScale.Begin();

            var nextY = DrawHearts();
            DrawInventoryToggleButton();
            DrawSkillsToggleButton();
            DrawJournalToggleButton();

            GUI.Label(new Rect(16f, nextY, 520f, 22f), $"{SceneManager.GetActiveScene().name}  |  Move: WASD/Arrows  Shoot: Space  Sprint: Shift  Inventory: I  Skills: K  Journal: J  All: Tab");
            nextY += 22f;

            nextY = DrawAmmo(nextY);

            DrawMoneyHud();
            DrawJobTracker();
            DrawHotbar();
            DrawEquipmentSlots();
            DrawReloadBar();
            DrawPickupPopup();

            var chestOpen = ChestInventory.ActiveChest != null;

            if (inventoryVisible || chestOpen)
            {
                DrawInventoryHud(nextY);
            }

            if (chestOpen)
            {
                DrawChestHud(nextY);
            }

            if (skillsVisible)
            {
                DrawSkillsHud(nextY);
            }

            if (journalVisible)
            {
                DrawJournalHud();
            }

            DrawDraggedItemPreview();

            if (Event.current.type == EventType.MouseUp)
            {
                _draggedInventorySlot = -1;
            }
        }

        private void DrawDraggedItemPreview()
        {
            if (_draggedInventorySlot < 0 || playerInventory == null || !playerInventory.IsValidSlot(_draggedInventorySlot))
            {
                return;
            }

            var slot = playerInventory.Slots[_draggedInventorySlot];
            if (slot.IsEmpty || slot.Item.icon == null)
            {
                return;
            }

            const float previewSize = 32f;
            var mousePosition = Event.current.mousePosition;
            var previewRect = new Rect(mousePosition.x - previewSize / 2f, mousePosition.y - previewSize / 2f, previewSize, previewSize);
            DrawSprite(previewRect, slot.Item.icon);
        }

        private float DrawHearts()
        {
            const float startX = 16f;
            const float startY = 16f;

            if (playerHealth == null)
            {
                return startY;
            }

            var heartCountValue = HeartCount;
            var rows = Mathf.CeilToInt(heartCountValue / (float)heartsPerRow);
            var rowHeight = heartSize + heartSpacing;

            var filledHalfUnits = playerHealth.MaxHealth > 0
                ? Mathf.RoundToInt(playerHealth.CurrentHealth / (float)playerHealth.MaxHealth * heartCountValue * 2)
                : 0;

            for (var heartIndex = 0; heartIndex < heartCountValue; heartIndex++)
            {
                var row = heartIndex / heartsPerRow;
                var column = heartIndex % heartsPerRow;
                var x = startX + column * (heartSize + heartSpacing);
                var y = startY + row * rowHeight;

                var remaining = filledHalfUnits - heartIndex * 2;
                var texture = remaining >= 2 ? fullHeartTexture : remaining == 1 ? halfHeartTexture : emptyHeartTexture;
                DrawSlot(new Rect(x, y, heartSize, heartSize), texture);
            }

            return startY + rows * rowHeight + 8f;
        }

        private float DrawAmmo(float y)
        {
            const float startX = 16f;

            if (playerShoot == null || playerShoot.CurrentAmmoType == global::WeaponAmmoType.None)
            {
                return y;
            }

            var (fullTexture, emptyTexture) = GetAmmoTextures(playerShoot.CurrentAmmoType);
            var maxAmmo = playerShoot.MaxAmmo;
            var currentAmmo = playerShoot.CurrentAmmo;
            var rows = Mathf.CeilToInt(maxAmmo / (float)ammoPerRow);
            var rowHeight = ammoIconSize + ammoIconSpacing;

            for (var ammoIndex = 0; ammoIndex < maxAmmo; ammoIndex++)
            {
                var row = ammoIndex / ammoPerRow;
                var column = ammoIndex % ammoPerRow;
                var x = startX + column * (ammoIconSize + ammoIconSpacing);
                var iconY = y + row * rowHeight;

                var texture = ammoIndex < currentAmmo ? fullTexture : emptyTexture;
                DrawSlot(new Rect(x, iconY, ammoIconSize, ammoIconSize), texture);
            }

            return y + rows * rowHeight + 16f;
        }

        private void DrawReloadBar()
        {
            if (playerShoot == null || !playerShoot.IsReloading)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var worldPosition = playerShoot.transform.position + Vector3.up * reloadBarWorldOffset;
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            var rect = new Rect(
                screenPoint.x - reloadBarWidth / 2f,
                Screen.height - screenPoint.y - reloadBarHeight / 2f,
                reloadBarWidth,
                reloadBarHeight);

            // screenPoint is already a real screen-pixel position from WorldToScreenPoint - draw it
            // outside the reference-resolution scale so it isn't shifted off the player.
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            DrawSlot(rect, reloadBarBackgroundTexture);

            var fillRect = new Rect(rect.x, rect.y, rect.width * playerShoot.ReloadFractionRemaining, rect.height);
            DrawSlot(fillRect, reloadBarFillTexture);

            GUI.Label(rect, "Reloading", CenteredLabelStyle);

            GUI.matrix = previousMatrix;
        }

        private void DrawPickupPopup()
        {
            if (_pickupPopupTimeRemaining <= 0f || string.IsNullOrEmpty(_pickupPopupText) || playerInventory == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var progress = 1f - Mathf.Clamp01(_pickupPopupTimeRemaining / pickupPopupDuration);
            var worldPosition = playerInventory.transform.position + Vector3.up * (pickupPopupWorldOffset + progress * pickupPopupRiseDistance);
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            const float width = 320f;
            const float height = 28f;
            var rect = new Rect(screenPoint.x - width / 2f, Screen.height - screenPoint.y - height / 2f, width, height);

            // screenPoint is already a real screen-pixel position from WorldToScreenPoint - draw it
            // outside the reference-resolution scale so it isn't shifted off the player.
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            // Plain GUI.Label has no outline, so fake one by stamping the text in black a
            // couple pixels off-center before drawing the green label on top — keeps it
            // readable over bright backgrounds instead of just relying on the bold green.
            var shadowStyle = new GUIStyle(PickupPopupStyle);
            shadowStyle.normal.textColor = Color.black;

            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 1f - progress);

            foreach (var offset in ShadowOffsets)
            {
                GUI.Label(new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height), _pickupPopupText, shadowStyle);
            }

            GUI.Label(rect, _pickupPopupText, PickupPopupStyle);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private static readonly Vector2[] ShadowOffsets =
        {
            new(-1f, -1f), new(1f, -1f), new(-1f, 1f), new(1f, 1f)
        };

        private (Texture2D full, Texture2D empty) GetAmmoTextures(global::WeaponAmmoType ammoType)
        {
            switch (ammoType)
            {
                case global::WeaponAmmoType.Pistol:
                    return (pistolAmmoFullTexture, pistolAmmoEmptyTexture);
                case global::WeaponAmmoType.Shotgun:
                    return (shotgunAmmoFullTexture, shotgunAmmoEmptyTexture);
                case global::WeaponAmmoType.Rifle:
                    return (rifleAmmoFullTexture, rifleAmmoEmptyTexture);
                default:
                    return (null, null);
            }
        }

        private void DrawMoneyHud()
        {
            DrawSlot(new Rect(GuiScale.ReferenceWidth - moneyWidth - 16f, 16f, moneyWidth, moneyHeight), moneyTexture);
        }

        private void DrawJobTracker()
        {
            var activeJobs = JobManager.ActiveJobs;
            if (activeJobs.Count == 0)
            {
                return;
            }

            var x = GuiScale.ReferenceWidth - jobTrackerWidth - 16f;
            var y = 16f + moneyHeight + 8f;

            foreach (var job in activeJobs)
            {
                var rect = new Rect(x, y, jobTrackerWidth, jobTrackerRowHeight);
                DrawSlot(rect, jobTrackerRowBackgroundTexture);
                GUI.Label(rect, $"{job.killTargetTag}s: {JobManager.GetProgress(job)}/{job.killTargetCount}", CenteredLabelStyle);
                y += jobTrackerRowHeight + jobTrackerRowGap;
            }
        }

        private void DrawEquipmentSlots()
        {
            if (playerInventory == null)
            {
                return;
            }

            const float margin = 16f;

            var weaponRect = new Rect(margin, GuiScale.ReferenceHeight - equipmentBoxSize - margin, equipmentBoxSize, equipmentBoxSize);
            var outfitRect = new Rect(margin + equipmentBoxSize + equipmentBoxGap, GuiScale.ReferenceHeight - equipmentBoxSize - margin, equipmentBoxSize, equipmentBoxSize);

            GUI.Label(new Rect(weaponRect.x, weaponRect.y - 18f, equipmentBoxSize, 18f), "Weapon");
            GUI.Label(new Rect(outfitRect.x, outfitRect.y - 18f, equipmentBoxSize, 18f), "Outfit");

            if (DrawEquipmentSlot(weaponRect, playerInventory.EquippedWeapon) && IsDoubleClick(WeaponBoxClickId))
            {
                playerInventory.TryUnequipWeapon();
            }

            if (DrawEquipmentSlot(outfitRect, playerInventory.EquippedArmor) && IsDoubleClick(OutfitBoxClickId))
            {
                playerInventory.TryUnequipArmor();
            }
        }

        private static bool DrawEquipmentSlot(Rect rect, global::Item item)
        {
            var clicked = GUI.Button(rect, string.Empty);
            if (item != null && item.icon != null)
            {
                DrawSprite(rect, item.icon);
            }
            return clicked;
        }

        private bool IsDoubleClick(int id)
        {
            var now = Time.unscaledTimeAsDouble;
            if (_lastClickId == id && now - _lastClickTime <= DoubleClickSeconds)
            {
                _lastClickId = int.MinValue;
                return true;
            }

            _lastClickId = id;
            _lastClickTime = now;
            return false;
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            var texture = sprite.texture;
            var textureRect = sprite.textureRect;
            var uv = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
            GUI.DrawTextureWithTexCoords(rect, texture, uv);
        }

        private void DrawHotbar()
        {
            const int slotCount = PlayerInventory.HotbarSlotCount;
            var totalWidth = slotCount * hotbarSlotSize + (slotCount - 1) * hotbarSlotGap;
            var startX = (GuiScale.ReferenceWidth - totalWidth) / 2f;
            var y = GuiScale.ReferenceHeight - hotbarSlotSize - 24f;

            for (var hotbarIndex = 0; hotbarIndex < slotCount; hotbarIndex++)
            {
                var x = startX + hotbarIndex * (hotbarSlotSize + hotbarSlotGap);
                var rect = new Rect(x, y, hotbarSlotSize, hotbarSlotSize);
                var texture = hotbarIndex < hotbarSlotTextures.Length ? hotbarSlotTextures[hotbarIndex] : null;

                DrawSlot(rect, texture);
                GUI.Label(new Rect(rect.x, rect.y - 16f, rect.width, 14f), (hotbarIndex + 1).ToString(), CenteredLabelStyle);

                var boundItem = playerInventory != null ? playerInventory.GetHotbarItem(hotbarIndex) : null;
                if (boundItem != null && boundItem.icon != null)
                {
                    DrawSprite(rect, boundItem.icon);
                }

                if (_draggedInventorySlot != -1 && Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
                {
                    var draggedItem = playerInventory.Slots[_draggedInventorySlot].Item;
                    playerInventory.SetHotbarItem(hotbarIndex, draggedItem);
                    _draggedInventorySlot = -1;
                }
            }
        }

        private static void DrawSlot(Rect rect, Texture2D texture)
        {
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Box(rect, string.Empty);
            }
        }

        private void DrawInventoryToggleButton()
        {
            var heartsRowWidth = Mathf.Min(HeartCount, heartsPerRow) * (heartSize + heartSpacing);
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
            var heartsRowWidth = Mathf.Min(HeartCount, heartsPerRow) * (heartSize + heartSpacing);
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
            var heartsRowWidth = Mathf.Min(HeartCount, heartsPerRow) * (heartSize + heartSpacing);
            var bagX = 16f + heartsRowWidth + 12f;
            var rect = new Rect(bagX + (bagButtonWidth + 8f) * 2f, 16f, bagButtonWidth, bagButtonHeight);

            var activeCount = JobManager.ActiveJobs.Count;
            var label = activeCount > 0 ? $"Journal ({activeCount})" : "Journal";

            if (GUI.Button(rect, label))
            {
                journalVisible = !journalVisible;
            }
        }

        private void DrawJournalHud()
        {
            var panelX = (GuiScale.ReferenceWidth - journalPanelWidth) / 2f;
            var panelY = (GuiScale.ReferenceHeight - journalPanelHeight) / 2f;

            DrawSlot(new Rect(panelX, panelY, journalPanelWidth, journalPanelHeight), journalPanelBackgroundTexture);
            GUI.Label(new Rect(panelX, panelY + 8f, journalPanelWidth, 28f), "Job Journal", CenteredLabelStyle);

            const float tabWidth = 140f;
            const float tabHeight = 28f;
            var tabY = panelY + 40f;
            var activeTabRect = new Rect(panelX + journalPanelWidth / 2f - tabWidth - 4f, tabY, tabWidth, tabHeight);
            var completedTabRect = new Rect(panelX + journalPanelWidth / 2f + 4f, tabY, tabWidth, tabHeight);

            var previousEnabled = GUI.enabled;
            GUI.enabled = journalTab != 0;
            if (GUI.Button(activeTabRect, "Active"))
            {
                journalTab = 0;
            }
            GUI.enabled = journalTab != 1;
            if (GUI.Button(completedTabRect, "Completed"))
            {
                journalTab = 1;
            }
            GUI.enabled = previousEnabled;

            var listX = panelX + 16f;
            var listY = tabY + tabHeight + 12f;
            var listWidth = journalPanelWidth - 32f;

            if (journalTab == 0)
            {
                DrawJournalActiveTab(listX, listY, listWidth);
            }
            else
            {
                DrawJournalCompletedTab(listX, listY, listWidth);
            }

            var closeRect = new Rect(panelX + journalPanelWidth - 76f, panelY + 8f, 60f, 24f);
            if (GUI.Button(closeRect, "Close"))
            {
                journalVisible = false;
            }
        }

        private void DrawJournalActiveTab(float x, float y, float width)
        {
            var activeJobs = JobManager.ActiveJobs;
            if (activeJobs.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, journalRowHeight), "No active jobs.");
                return;
            }

            var rowY = y;
            foreach (var job in activeJobs)
            {
                GUI.Label(new Rect(x, rowY, width, 20f), job.jobName);
                var objective = $"Kill {job.killTargetTag}s: {JobManager.GetProgress(job)}/{job.killTargetCount}";
                GUI.Label(new Rect(x, rowY + 20f, width, 20f), objective);
                rowY += journalRowHeight;
            }
        }

        private void DrawJournalCompletedTab(float x, float y, float width)
        {
            var finishedJobs = JobManager.FinishedJobs;
            if (finishedJobs.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, journalRowHeight), "No jobs completed yet.");
                return;
            }

            var rowY = y;
            foreach (var job in finishedJobs)
            {
                GUI.Label(new Rect(x, rowY, width, 20f), $"{job.jobName}  —  Complete");
                rowY += journalRowHeight;
            }
        }

        private void DrawSkillsHud(float startY)
        {
            if (characterSheet == null)
            {
                return;
            }

            var skills = (SkillType[])System.Enum.GetValues(typeof(SkillType));
            var startX = GuiScale.ReferenceWidth - skillsPanelWidth - 16f;
            var height = 28f + skills.Length * skillRowHeight + 8f;

            DrawSlot(new Rect(startX, startY, skillsPanelWidth, height), skillsPanelBackgroundTexture);

            GUI.Label(new Rect(startX + 8f, startY + 4f, skillsPanelWidth - 16f, 20f),
                $"Level {characterSheet.Level}   XP {characterSheet.Experience}/{characterSheet.ExperienceToNextLevel}   Points: {characterSheet.UnspentSkillPoints}");

            var rowY = startY + 28f;
            foreach (var skill in skills)
            {
                var skillLevel = characterSheet.GetLevel(skill);
                GUI.Label(new Rect(startX + 8f, rowY, skillsPanelWidth - 56f, skillRowHeight), $"{skill}: {skillLevel}/{CharacterSheet.MaxSkillLevel}");

                var canAllocate = characterSheet.UnspentSkillPoints > 0 && skillLevel < CharacterSheet.MaxSkillLevel;
                GUI.enabled = canAllocate;
                if (GUI.Button(new Rect(startX + skillsPanelWidth - 40f, rowY, 32f, skillRowHeight - 4f), "+"))
                {
                    characterSheet.TryAllocateSkillPoint(skill);
                }
                GUI.enabled = true;

                rowY += skillRowHeight;
            }
        }

        private void DrawInventoryHud(float startY)
        {
            if (playerInventory == null)
            {
                return;
            }

            const float startX = 16f;
            var width = inventoryPanelWidth;
            var height = inventoryPanelHeight;
            var gridStartX = startX + 12f;
            var gridStartY = startY + inventoryGridTopPadding;

            DrawSlot(new Rect(startX, startY, width, height), inventoryBorderTexture);

            var slots = playerInventory.Slots;
            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var row = slotIndex / InventoryColumns;
                var column = slotIndex % InventoryColumns;
                var slotX = gridStartX + column * (inventorySlotWidth + inventorySlotGap);
                var slotY = gridStartY + row * (inventorySlotHeight + inventorySlotGap);
                var slotRect = new Rect(slotX, slotY, inventorySlotWidth, inventorySlotHeight);
                var slot = slots[slotIndex];
                var slotLabel = BuildSlotLabel(slot);

                if (Event.current.type == EventType.MouseDown && slotRect.Contains(Event.current.mousePosition) &&
                    !slot.IsEmpty && IsHotbarEligible(slot.Item))
                {
                    _draggedInventorySlot = slotIndex;
                }

                bool clicked;
                if (inventorySlotBackgroundTexture != null)
                {
                    clicked = GUI.Button(slotRect, string.Empty);
                    GUI.DrawTexture(slotRect, inventorySlotBackgroundTexture, ScaleMode.ScaleToFit);
                    if (!string.IsNullOrEmpty(slotLabel))
                    {
                        GUI.Label(slotRect, slotLabel, CenteredLabelStyle);
                    }
                }
                else
                {
                    clicked = GUI.Button(slotRect, slotLabel);
                }

                if (clicked)
                {
                    if (!slot.IsEmpty && PlayerInventory.IsEquipment(slot.Item))
                    {
                        if (IsDoubleClick(slotIndex))
                        {
                            playerInventory.TryEquipAt(slotIndex);
                        }
                    }
                    else
                    {
                        selectedInventorySlot = slotIndex;
                    }
                }
            }

            var rowCount = Mathf.CeilToInt(slots.Count / (float)InventoryColumns);
            var gridBottomY = gridStartY + rowCount * (inventorySlotHeight + inventorySlotGap);
            DrawSelectedItemPanel(startX + 12f, gridBottomY + 8f, width - 24f);

            if (!string.IsNullOrWhiteSpace(playerInventory.LastMessage))
            {
                GUI.Label(new Rect(startX + 12f, startY + height - 26f, width - 24f, 20f), playerInventory.LastMessage);
            }
        }

        // Mirrors DrawInventoryHud's grid rendering, reading from the currently open ChestInventory
        // instead of the player's own inventory. Click-to-take only (no drag-and-drop) - clicking a
        // slot hands its whole stack to the player and clears it from the chest.
        private void DrawChestHud(float startY)
        {
            var chest = ChestInventory.ActiveChest;
            if (chest == null || playerInventory == null)
            {
                return;
            }

            const int chestColumns = 5;
            const float margin = 16f;
            var width = inventoryPanelWidth;
            var startX = GuiScale.ReferenceWidth - width - margin;
            var gridStartX = startX + 12f;
            var gridStartY = startY + inventoryGridTopPadding;

            var slots = chest.Slots;
            var rowCount = Mathf.Max(1, Mathf.CeilToInt(slots.Count / (float)chestColumns));
            var height = inventoryGridTopPadding + rowCount * (inventorySlotHeight + inventorySlotGap) + 16f;

            DrawSlot(new Rect(startX, startY, width, height), inventoryBorderTexture);
            GUI.Label(new Rect(startX + 12f, startY + 4f, width - 24f, 20f), "Chest (click to take)");

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (slot.IsEmpty)
                {
                    continue;
                }

                var row = slotIndex / chestColumns;
                var column = slotIndex % chestColumns;
                var slotX = gridStartX + column * (inventorySlotWidth + inventorySlotGap);
                var slotY = gridStartY + row * (inventorySlotHeight + inventorySlotGap);
                var slotRect = new Rect(slotX, slotY, inventorySlotWidth, inventorySlotHeight);
                var slotLabel = BuildSlotLabel(slot);

                bool clicked;
                if (inventorySlotBackgroundTexture != null)
                {
                    clicked = GUI.Button(slotRect, string.Empty);
                    GUI.DrawTexture(slotRect, inventorySlotBackgroundTexture, ScaleMode.ScaleToFit);
                    if (!string.IsNullOrEmpty(slotLabel))
                    {
                        GUI.Label(slotRect, slotLabel, CenteredLabelStyle);
                    }
                }
                else
                {
                    clicked = GUI.Button(slotRect, slotLabel);
                }

                if (clicked && playerInventory.TryAdd(slot.Item, slot.Amount))
                {
                    chest.ClearSlot(slotIndex);
                }
            }
        }

        private void DrawSelectedItemPanel(float startX, float startY, float width)
        {
            if (!playerInventory.IsValidSlot(selectedInventorySlot))
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Select an item.");
                return;
            }

            var selectedSlot = playerInventory.Slots[selectedInventorySlot];
            if (selectedSlot.IsEmpty)
            {
                GUI.Label(new Rect(startX, startY, width, 20f), "Empty");
                return;
            }

            var selectedItem = selectedSlot.Item;
            var itemName = GetItemName(selectedItem);
            var countText = selectedSlot.Amount > 1 ? $" x{selectedSlot.Amount}" : string.Empty;

            GUI.Label(new Rect(startX, startY, width, 20f), $"{itemName}{countText} ({selectedItem.itemType})");
            GUI.Label(new Rect(startX, startY + 20f, width, 20f), selectedItem.description);

            var buttonX = startX;
            var buttonY = startY + 44f;
            if (playerInventory.CanUseAt(selectedInventorySlot))
            {
                if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Use"))
                {
                    playerInventory.TryUseAt(selectedInventorySlot, playerHealth);
                }

                buttonX += 70f;
            }

            if (GUI.Button(new Rect(buttonX, buttonY, 64f, 24f), "Drop"))
            {
                if (playerInventory.TryDropAt(selectedInventorySlot))
                {
                    var slotStillValid = playerInventory.IsValidSlot(selectedInventorySlot);
                    if (!slotStillValid || playerInventory.Slots[selectedInventorySlot].IsEmpty)
                    {
                        selectedInventorySlot = -1;
                    }
                }
            }
        }

        private static string BuildSlotLabel(PlayerInventory.InventorySlot slot)
        {
            if (slot.IsEmpty)
            {
                return string.Empty;
            }

            var amountText = slot.Amount > 1 ? $" x{slot.Amount}" : string.Empty;
            return $"{GetItemName(slot.Item)}{amountText}";
        }

        private static string GetItemName(global::Item item)
        {
            if (item == null)
            {
                return "Empty";
            }

            return string.IsNullOrWhiteSpace(item.displayName) ? item.name : item.displayName;
        }

        private static bool IsHotbarEligible(global::Item item)
        {
            return item != null &&
                   (PlayerInventory.IsEquipment(item) || item.itemType == global::ItemType.Consumable);
        }
    }
}
