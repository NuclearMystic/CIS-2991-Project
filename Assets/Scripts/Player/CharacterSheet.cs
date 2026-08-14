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

        [Header("Leveling — 7 skills x 100 max level = 700 points to fully master every skill.")]
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

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

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
            return _skillLevels.TryGetValue(skill, out var level) ? level : MinSkillLevel;
        }

        public void SetLevel(SkillType skill, int level)
        {
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
            return 1f + GetLevel(SkillType.Athletics) * athleticsSpeedPerLevel;
        }

        public float GetWeaponDamageMultiplier(SkillType weaponSkill)
        {
            return 1f + GetLevel(weaponSkill) * weaponDamagePerLevel;
        }

        public float GetWeaponReloadMultiplier(SkillType weaponSkill)
        {
            return Mathf.Max(1f - GetLevel(weaponSkill) * weaponReloadSpeedPerLevel, minReloadMultiplier);
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
