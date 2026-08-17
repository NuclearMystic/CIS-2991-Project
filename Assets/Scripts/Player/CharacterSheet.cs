using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.Player
{
    // Add new skills here — the storage/leveling API below (GetLevel/SetLevel/AddLevels) picks
    // them up automatically. Wiring an actual gameplay effect for a new skill still has to be
    // added wherever that system lives (see PlayerShoot/PlayerMovement/PlayerHealth for examples).
    public enum SkillType
    {
        Pistol,
        Rifle,
        Shotgun,
        Melee,
        Endurance,
        Barter,
        Athletics,
        Lockpicking,
    }

    public class CharacterSheet : MonoBehaviour
    {
        public const int MinSkillLevel = 0;
        public const int MaxSkillLevel = 100;
        public const int MaxCharacterLevel = 100;

        [Serializable]
        private struct SkillEntry
        {
            public SkillType skill;
            [Range(MinSkillLevel, MaxSkillLevel)] public int level;
        }

        [Header("Starting Skills — any skill not listed here starts at 0")]
        [SerializeField]
        private List<SkillEntry> startingSkills = new List<SkillEntry>
        {
            new SkillEntry { skill = SkillType.Endurance, level = 10 },
        };

        [Header("Endurance — hearts")]
        [Tooltip("Total hearts = Endurance level / this value, floored at 1 heart.")]
        [SerializeField, Min(1)] private int endurancePerHeart = 5;

        [Header("Athletics — movement speed")]
        [Tooltip("Move speed multiplier = 1 + (Athletics level * this value). Default 1% per level -> 2x speed at level 100.")]
        [SerializeField] private float athleticsSpeedPerLevel = 0.01f;

        [Header("Weapon Skills (Pistol/Rifle/Shotgun) — damage and reload speed")]
        [Tooltip("Damage multiplier = 1 + (skill level * this value).")]
        [SerializeField] private float weaponDamagePerLevel = 0.005f;
        [Tooltip("Reload time multiplier = 1 - (skill level * this value), floored at Min Reload Multiplier.")]
        [SerializeField] private float weaponReloadSpeedPerLevel = 0.005f;
        [SerializeField, Range(0.05f, 1f)] private float minReloadMultiplier = 0.1f;

        [Header("Leveling — 8 skills x 100 max level = 800 points to fully master every skill.")]
        [Tooltip("Skill points granted each time the character levels up. At 3/level x 99 level-ups (1->100) " +
                 "that's ~297 points by max level: enough to fully master 2-3 skills, or spread thinner across all 7 " +
                 "- meaningful specialization without letting one playthrough max everything.")]
        [SerializeField, Min(0)] private int skillPointsPerLevel = 3;
        [Tooltip("Experience required to go from level 1 to level 2.")]
        [SerializeField, Min(1f)] private float baseExperienceToLevel = 25f;
        [Tooltip("How much more experience each subsequent level requires than the last.")]
        [SerializeField, Min(0f)] private float experienceIncreasePerLevel = 5f;

        [SerializeField, Min(1)] private int characterLevel = 1;
        [SerializeField, Min(0)] private int experience;
        [SerializeField, Min(0)] private int unspentSkillPoints;

        private readonly Dictionary<SkillType, int> _skillLevels = new Dictionary<SkillType, int>();
        private SurvivalStats _survivalStats;

        public event Action<SkillType, int> SkillChanged;
        public event Action<int> LevelChanged;
        public event Action<int, int> ExperienceChanged;
        public event Action<int> SkillPointsChanged;

        public string PreviousScene { get; set; }

        public int Level => characterLevel;
        public int Experience => experience;
        public int UnspentSkillPoints => unspentSkillPoints;
        public bool IsMaxLevel => characterLevel >= MaxCharacterLevel;
        public int ExperienceToNextLevel => IsMaxLevel ? 0 : Mathf.RoundToInt(GetExperienceRequirement(characterLevel));

        private bool _skillLevelsInitialized;

        private void Awake()
        {
            _survivalStats = GetComponent<SurvivalStats>();
            EnsureSkillLevelsInitialized();
        }

        // Unity doesn't guarantee Awake() order between components on the same GameObject - other
        // components (e.g. PlayerHealth, reading GetHeartCount() for its own Awake) can query skill
        // levels before this component's own Awake has run. Called at the top of every read/write
        // entry point below so _skillLevels is always populated by the time it's needed, regardless
        // of ordering.
        private void EnsureSkillLevelsInitialized()
        {
            if (_skillLevelsInitialized)
            {
                return;
            }

            _skillLevelsInitialized = true;

            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                _skillLevels[skill] = MinSkillLevel;
            }

            foreach (var entry in startingSkills)
            {
                _skillLevels[entry.skill] = Mathf.Clamp(entry.level, MinSkillLevel, MaxSkillLevel);
            }
        }

        public int GetLevel(SkillType skill)
        {
            EnsureSkillLevelsInitialized();
            return _skillLevels.TryGetValue(skill, out var level) ? level : MinSkillLevel;
        }

        public void SetLevel(SkillType skill, int level)
        {
            EnsureSkillLevelsInitialized();
            var clamped = Mathf.Clamp(level, MinSkillLevel, MaxSkillLevel);
            if (_skillLevels.TryGetValue(skill, out var current) && current == clamped)
            {
                return;
            }

            _skillLevels[skill] = clamped;
            SkillChanged?.Invoke(skill, clamped);
        }

        public void AddLevels(SkillType skill, int amount)
        {
            SetLevel(skill, GetLevel(skill) + amount);
        }

        // Restores previously-saved progression in one shot rather than replaying it level-by-level -
        // used by SaveSystem when loading a save. Fires the normal change events once at the end so
        // dependent systems (PlayerHealth's Endurance-driven max HP, the Skills panel, etc.) refresh.
        public void LoadState(int savedLevel, int savedExperience, int savedSkillPoints, string savedPreviousScene, IReadOnlyDictionary<SkillType, int> savedSkillLevels)
        {
            EnsureSkillLevelsInitialized();
            characterLevel = Mathf.Clamp(savedLevel, 1, MaxCharacterLevel);
            experience = Mathf.Max(0, savedExperience);
            unspentSkillPoints = Mathf.Max(0, savedSkillPoints);
            PreviousScene = savedPreviousScene;

            if (savedSkillLevels != null)
            {
                foreach (var pair in savedSkillLevels)
                {
                    _skillLevels[pair.Key] = Mathf.Clamp(pair.Value, MinSkillLevel, MaxSkillLevel);
                }
            }

            LevelChanged?.Invoke(characterLevel);
            ExperienceChanged?.Invoke(experience, ExperienceToNextLevel);
            SkillPointsChanged?.Invoke(unspentSkillPoints);

            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                SkillChanged?.Invoke(skill, GetLevel(skill));
            }
        }

        public float GetNormalized(SkillType skill)
        {
            return GetLevel(skill) / (float)MaxSkillLevel;
        }

        public int GetHeartCount()
        {
            return Mathf.Max(GetLevel(SkillType.Endurance) / endurancePerHeart, 1);
        }

        public float GetMoveSpeedMultiplier()
        {
            return 1f + GetLevel(SkillType.Athletics) * athleticsSpeedPerLevel * GetSkillEffectivenessMultiplier();
        }

        public float GetWeaponDamageMultiplier(SkillType weaponSkill)
        {
            return 1f + GetLevel(weaponSkill) * weaponDamagePerLevel * GetSkillEffectivenessMultiplier();
        }

        public float GetWeaponReloadMultiplier(SkillType weaponSkill)
        {
            return Mathf.Max(1f - GetLevel(weaponSkill) * weaponReloadSpeedPerLevel * GetSkillEffectivenessMultiplier(), minReloadMultiplier);
        }

        // Starving/dehydrated (SurvivalStats) cuts how much benefit skill levels actually provide,
        // without touching the stored levels themselves (those still show correctly in the Skills UI).
        private float GetSkillEffectivenessMultiplier()
        {
            return _survivalStats != null ? _survivalStats.SkillEffectivenessMultiplier : 1f;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
            {
                return;
            }

            experience += amount;
            var leveledUp = false;

            while (!IsMaxLevel && experience >= GetExperienceRequirement(characterLevel))
            {
                experience -= Mathf.RoundToInt(GetExperienceRequirement(characterLevel));
                characterLevel++;
                unspentSkillPoints += skillPointsPerLevel;
                leveledUp = true;
                LevelChanged?.Invoke(characterLevel);
            }

            if (IsMaxLevel)
            {
                experience = 0;
            }

            ExperienceChanged?.Invoke(experience, ExperienceToNextLevel);

            if (leveledUp)
            {
                SkillPointsChanged?.Invoke(unspentSkillPoints);
            }
        }

        public bool TryAllocateSkillPoint(SkillType skill)
        {
            if (unspentSkillPoints <= 0 || GetLevel(skill) >= MaxSkillLevel)
            {
                return false;
            }

            unspentSkillPoints--;
            AddLevels(skill, 1);
            SkillPointsChanged?.Invoke(unspentSkillPoints);
            return true;
        }

        private float GetExperienceRequirement(int forLevel)
        {
            return baseExperienceToLevel + experienceIncreasePerLevel * (forLevel - 1);
        }
    }
}
