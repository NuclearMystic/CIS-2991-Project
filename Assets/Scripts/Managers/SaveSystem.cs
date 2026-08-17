using System;
using System.Collections.Generic;
using System.IO;
using CIS2991Project.Core;
using CIS2991Project.Data;
using CIS2991Project.Jobs;
using CIS2991Project.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.Managers
{
    // JSON-based save/load. JsonUtility (the only JSON option in this project - no Newtonsoft package
    // installed) can't serialize Dictionary<K,V> or top-level arrays, so everything here is arrays of
    // small structs instead - the same shape CharacterSheet.SkillEntry already uses for startingSkills.
    // Item/JobDefinition references are saved as their stable string id (see Item.id) and resolved
    // back through ItemDatabase on load, since the ScriptableObjects themselves can't be embedded in
    // a JSON file. Player progress only - chests/enemies have no persistence to build on today (see
    // the save/load plan notes), so world state isn't part of this.
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        private const string SaveFolderName = "Saves";

        // Set true for the one frame a screenshot is being captured, so IMGUI panels
        // (PauseMenuController) can skip drawing themselves out of the shot.
        public static bool IsCapturingScreenshot { get; set; }

        [Serializable]
        private class SaveFile
        {
            public string sceneName;
            public string timestamp;
            public float playerPositionX;
            public float playerPositionY;

            public int characterLevel;
            public int experience;
            public int unspentSkillPoints;
            public string previousScene;
            public SkillSaveEntry[] skills;

            public int currentHealth;

            public InventorySlotSaveData[] slots;
            public string equippedWeaponId;
            public string equippedArmorId;
            public string[] hotbarItemIds;
            public int currency;

            public float hunger;
            public float thirst;

            public int currentAmmo;
            public int facingDirection;

            public JobProgressSaveData[] activeJobs;
            public string[] completedJobIds;
        }

        [Serializable]
        private struct SkillSaveEntry
        {
            public SkillType skill;
            public int level;
        }

        [Serializable]
        private struct InventorySlotSaveData
        {
            public string itemId;
            public int amount;
        }

        [Serializable]
        private struct JobProgressSaveData
        {
            public string jobId;
            public int progress;
        }

        public readonly struct SlotInfo
        {
            public readonly bool Exists;
            public readonly string SceneName;
            public readonly DateTime Timestamp;
            public readonly Texture2D Thumbnail;

            public SlotInfo(bool exists, string sceneName, DateTime timestamp, Texture2D thumbnail)
            {
                Exists = exists;
                SceneName = sceneName;
                Timestamp = timestamp;
                Thumbnail = thumbnail;
            }
        }

        private static SaveFile _pendingLoad;

        private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);

        private static string JsonPath(int slotIndex) => Path.Combine(SaveFolderPath, $"slot{slotIndex}.json");

        private static string ThumbnailPath(int slotIndex) => Path.Combine(SaveFolderPath, $"slot{slotIndex}.png");

        public static SlotInfo GetSlotInfo(int slotIndex)
        {
            var jsonPath = JsonPath(slotIndex);
            if (!File.Exists(jsonPath))
            {
                return new SlotInfo(false, null, default, null);
            }

            SaveFile data;
            try
            {
                data = JsonUtility.FromJson<SaveFile>(File.ReadAllText(jsonPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveSystem: failed to read slot {slotIndex}: {e.Message}");
                return new SlotInfo(false, null, default, null);
            }

            Texture2D thumbnail = null;
            var thumbnailPath = ThumbnailPath(slotIndex);
            if (File.Exists(thumbnailPath))
            {
                thumbnail = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                thumbnail.LoadImage(File.ReadAllBytes(thumbnailPath));
            }

            var timestamp = DateTime.TryParse(data.timestamp, out var parsed) ? parsed : DateTime.MinValue;
            return new SlotInfo(true, data.sceneName, timestamp, thumbnail);
        }

        // Gathers live state via FindAnyObjectByType - same pattern already used by GameOverController/
        // PlayerHUD to reach player components without a wired-up reference.
        public static void Save(int slotIndex, Texture2D screenshot)
        {
            var playerHealth = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            var inventory = UnityEngine.Object.FindAnyObjectByType<PlayerInventory>();
            var characterSheet = UnityEngine.Object.FindAnyObjectByType<CharacterSheet>();
            var survivalStats = UnityEngine.Object.FindAnyObjectByType<SurvivalStats>();
            var playerShoot = UnityEngine.Object.FindAnyObjectByType<PlayerShoot>();
            var animationController = UnityEngine.Object.FindAnyObjectByType<PlayerAnimationController>();

            if (playerHealth == null || inventory == null || characterSheet == null)
            {
                Debug.LogWarning("SaveSystem: no player found, can't save.");
                return;
            }

            var data = new SaveFile
            {
                sceneName = SceneManager.GetActiveScene().name,
                timestamp = DateTime.Now.ToString("o"),
                playerPositionX = playerHealth.transform.position.x,
                playerPositionY = playerHealth.transform.position.y,

                characterLevel = characterSheet.Level,
                experience = characterSheet.Experience,
                unspentSkillPoints = characterSheet.UnspentSkillPoints,
                previousScene = characterSheet.PreviousScene,
                skills = BuildSkillEntries(characterSheet),

                currentHealth = playerHealth.CurrentHealth,

                slots = BuildInventoryEntries(inventory),
                equippedWeaponId = inventory.EquippedWeapon != null ? inventory.EquippedWeapon.id : null,
                equippedArmorId = inventory.EquippedArmor != null ? inventory.EquippedArmor.id : null,
                hotbarItemIds = BuildHotbarIds(inventory),
                currency = inventory.Currency,

                hunger = survivalStats != null ? survivalStats.Hunger : SurvivalStats.MaxMeter,
                thirst = survivalStats != null ? survivalStats.Thirst : SurvivalStats.MaxMeter,

                currentAmmo = playerShoot != null ? playerShoot.CurrentAmmo : 0,
                facingDirection = animationController != null ? (int)animationController.Facing : (int)Direction.Down,

                activeJobs = BuildActiveJobEntries(),
                completedJobIds = BuildCompletedJobIds(),
            };

            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(JsonPath(slotIndex), JsonUtility.ToJson(data));

            if (screenshot != null)
            {
                File.WriteAllBytes(ThumbnailPath(slotIndex), screenshot.EncodeToPNG());
            }

            Debug.Log($"[SaveDebug] Save: slot={slotIndex} scene={data.sceneName} pos=({data.playerPositionX:F1},{data.playerPositionY:F1}) level={data.characterLevel} hp={data.currentHealth} currency={data.currency} slotsWithItems={CountFilledSlots(data.slots)} player instanceID={playerHealth.GetInstanceID()}.");
        }

        private static int CountFilledSlots(InventorySlotSaveData[] slots)
        {
            if (slots == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var slot in slots)
            {
                if (!string.IsNullOrEmpty(slot.itemId))
                {
                    count++;
                }
            }

            return count;
        }

        // Reads the slot and queues it as a pending load, then triggers the scene switch. The actual
        // apply-to-player step happens later, once GameBootstrapper's normal post-scene-load flow has
        // a player to apply it to - see TryConsumePendingLoad.
        public static void Load(int slotIndex)
        {
            var jsonPath = JsonPath(slotIndex);
            if (!File.Exists(jsonPath))
            {
                return;
            }

            _pendingLoad = JsonUtility.FromJson<SaveFile>(File.ReadAllText(jsonPath));
            Debug.Log($"[SaveDebug] Load: slot={slotIndex} scene={_pendingLoad.sceneName} pos=({_pendingLoad.playerPositionX:F1},{_pendingLoad.playerPositionY:F1}) level={_pendingLoad.characterLevel} hp={_pendingLoad.currentHealth} currency={_pendingLoad.currency} slotsWithItems={CountFilledSlots(_pendingLoad.slots)} - queued, loading scene now.");
            SceneManager.LoadScene(_pendingLoad.sceneName);
        }

        public static bool TryConsumePendingLoad(PlayerHealth playerHealth, PlayerInventory inventory, CharacterSheet characterSheet,
            SurvivalStats survivalStats, PlayerShoot playerShoot, PlayerAnimationController animationController)
        {
            if (_pendingLoad == null)
            {
                Debug.Log("[SaveDebug] TryConsumePendingLoad: nothing pending, no-op.");
                return false;
            }

            var data = _pendingLoad;
            _pendingLoad = null;
            Debug.Log($"[SaveDebug] TryConsumePendingLoad: applying scene={data.sceneName} level={data.characterLevel} hp={data.currentHealth} currency={data.currency} slotsWithItems={CountFilledSlots(data.slots)} to player instanceID={playerHealth.GetInstanceID()}.");

            var database = LoadDatabase();

            playerHealth.transform.position = new Vector3(data.playerPositionX, data.playerPositionY, playerHealth.transform.position.z);

            var skillLevels = new Dictionary<SkillType, int>();
            if (data.skills != null)
            {
                foreach (var entry in data.skills)
                {
                    skillLevels[entry.skill] = entry.level;
                }
            }
            characterSheet.LoadState(data.characterLevel, data.experience, data.unspentSkillPoints, data.previousScene, skillLevels);

            playerHealth.LoadState(data.currentHealth);

            var slots = new List<(global::Item item, int amount)>();
            if (data.slots != null)
            {
                foreach (var entry in data.slots)
                {
                    slots.Add((database != null ? database.GetItemById(entry.itemId) : null, entry.amount));
                }
            }

            var hotbar = new List<global::Item>();
            if (data.hotbarItemIds != null)
            {
                foreach (var id in data.hotbarItemIds)
                {
                    hotbar.Add(database != null ? database.GetItemById(id) : null);
                }
            }

            inventory.LoadState(
                slots,
                database != null ? database.GetItemById(data.equippedWeaponId) : null,
                database != null ? database.GetItemById(data.equippedArmorId) : null,
                hotbar,
                data.currency);

            // After inventory.LoadState, since re-equipping a weapon there refills ammo via
            // PlayerShoot's own EquipmentChanged handler - this overrides that with the saved amount.
            if (playerShoot != null)
            {
                playerShoot.LoadState(data.currentAmmo);
            }

            if (animationController != null)
            {
                animationController.LoadState((Direction)data.facingDirection);
            }

            if (survivalStats != null)
            {
                survivalStats.LoadState(data.hunger, data.thirst);
            }

            var activeJobs = new List<(JobDefinition job, int progress)>();
            if (data.activeJobs != null && database != null)
            {
                foreach (var entry in data.activeJobs)
                {
                    var job = database.GetJobById(entry.jobId);
                    if (job != null)
                    {
                        activeJobs.Add((job, entry.progress));
                    }
                }
            }

            var completedJobs = new List<JobDefinition>();
            if (data.completedJobIds != null && database != null)
            {
                foreach (var id in data.completedJobIds)
                {
                    var job = database.GetJobById(id);
                    if (job != null)
                    {
                        completedJobs.Add(job);
                    }
                }
            }

            JobManager.LoadState(activeJobs, completedJobs);

            Debug.Log($"[SaveDebug] TryConsumePendingLoad: done - player instanceID={playerHealth.GetInstanceID()} now reports pos={playerHealth.transform.position} hp={playerHealth.CurrentHealth} level={characterSheet.Level} currency={inventory.Currency}.");

            return true;
        }

        private static ItemDatabase LoadDatabase()
        {
            var database = Resources.Load<ItemDatabase>("ItemDatabase");
            if (database == null)
            {
                Debug.LogWarning("SaveSystem: no ItemDatabase found at Resources/ItemDatabase - items/jobs won't resolve.");
            }

            return database;
        }

        private static SkillSaveEntry[] BuildSkillEntries(CharacterSheet characterSheet)
        {
            var skillTypes = (SkillType[])Enum.GetValues(typeof(SkillType));
            var entries = new SkillSaveEntry[skillTypes.Length];
            for (var i = 0; i < skillTypes.Length; i++)
            {
                entries[i] = new SkillSaveEntry { skill = skillTypes[i], level = characterSheet.GetLevel(skillTypes[i]) };
            }

            return entries;
        }

        private static InventorySlotSaveData[] BuildInventoryEntries(PlayerInventory inventory)
        {
            var slots = inventory.Slots;
            var entries = new InventorySlotSaveData[slots.Count];
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                entries[i] = slot.IsEmpty
                    ? new InventorySlotSaveData { itemId = null, amount = 0 }
                    : new InventorySlotSaveData { itemId = slot.Item.id, amount = slot.Amount };
            }

            return entries;
        }

        private static string[] BuildHotbarIds(PlayerInventory inventory)
        {
            var ids = new string[PlayerInventory.HotbarSlotCount];
            for (var i = 0; i < ids.Length; i++)
            {
                var item = inventory.GetHotbarItem(i);
                ids[i] = item != null ? item.id : null;
            }

            return ids;
        }

        private static JobProgressSaveData[] BuildActiveJobEntries()
        {
            var activeJobs = JobManager.ActiveJobs;
            var entries = new JobProgressSaveData[activeJobs.Count];
            var i = 0;
            foreach (var job in activeJobs)
            {
                entries[i] = new JobProgressSaveData { jobId = job.id, progress = JobManager.GetProgress(job) };
                i++;
            }

            return entries;
        }

        private static string[] BuildCompletedJobIds()
        {
            var finishedJobs = JobManager.FinishedJobs;
            var ids = new string[finishedJobs.Count];
            var i = 0;
            foreach (var job in finishedJobs)
            {
                ids[i] = job.id;
                i++;
            }

            return ids;
        }
    }
}
