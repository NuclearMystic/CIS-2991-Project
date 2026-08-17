using System;
using UnityEngine;

namespace CIS2991Project.Player
{
    // Hunger and thirst meters (0-100, start full). Both deplete steadily over time and combine into
    // one "severity" value (0 = both full, 1 = both empty) that drives the continuous speed/haze
    // debuffs - each meter alone can only push severity to 0.5, so being critically low on just one
    // is half as bad as being empty on both. Separately, a flat 25%-per-empty-meter skill debuff (see
    // CharacterSheet) and a tiny health drain both kick in only once a meter actually bottoms out at 0.
    public class SurvivalStats : MonoBehaviour
    {
        public const float MaxMeter = 100f;

        [Header("Depletion — seconds to drain fully from 100 to 0")]
        [SerializeField, Min(1f)] private float hungerDepletionSeconds = 900f;
        [SerializeField, Min(1f)] private float thirstDepletionSeconds = 720f;

        [Header("Speed Penalty — multiplies move speed alongside CharacterSheet's Athletics bonus")]
        [SerializeField, Range(0f, 1f)] private float maxSpeedPenaltyFraction = 0.5f;

        [Header("Starvation/Dehydration Drain — applied per empty meter, so both empty drains twice as fast")]
        [SerializeField, Min(0)] private int drainAmountPerTick = 1;
        [SerializeField, Min(0.1f)] private float drainIntervalSeconds = 10f;

        [Header("Skill Debuff — cut per empty meter, applied in CharacterSheet")]
        [SerializeField, Range(0f, 1f)] private float skillDebuffPerEmptyMeter = 0.25f;

        [Header("Debug — hold to rapidly drain a meter for testing")]
        [SerializeField] private KeyCode debugDrainHungerKey = KeyCode.T;
        [SerializeField] private KeyCode debugDrainThirstKey = KeyCode.G;
        [SerializeField, Min(0f)] private float debugDrainPerSecond = 25f;

        private PlayerHealth _playerHealth;
        private float _hunger = MaxMeter;
        private float _thirst = MaxMeter;
        private float _drainTimeRemaining;

        public event Action<float, float> HungerChanged;
        public event Action<float, float> ThirstChanged;

        public float Hunger => _hunger;
        public float Thirst => _thirst;

        // 0 (both meters full) to 1 (both meters empty) - each meter contributes up to half.
        public float Severity => (1f - _hunger / MaxMeter) * 0.5f + (1f - _thirst / MaxMeter) * 0.5f;

        public float SpeedMultiplier => 1f - Severity * maxSpeedPenaltyFraction;

        private int EmptyMeterCount => (_hunger <= 0f ? 1 : 0) + (_thirst <= 0f ? 1 : 0);

        // Read by CharacterSheet to cut the skill-derived portion of its move-speed/damage/reload
        // multipliers - see GetMoveSpeedMultiplier/GetWeaponDamageMultiplier/GetWeaponReloadMultiplier.
        public float SkillEffectivenessMultiplier => 1f - skillDebuffPerEmptyMeter * EmptyMeterCount;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _drainTimeRemaining = drainIntervalSeconds;
        }

        private void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            _hunger = Deplete(_hunger, hungerDepletionSeconds, debugDrainHungerKey);
            _thirst = Deplete(_thirst, thirstDepletionSeconds, debugDrainThirstKey);
            TickDrain();
        }

        private float Deplete(float meter, float depletionSeconds, KeyCode debugDrainKey)
        {
            if (meter <= 0f)
            {
                return 0f;
            }

            var rate = MaxMeter / depletionSeconds;
            if (Input.GetKey(debugDrainKey))
            {
                rate += debugDrainPerSecond;
            }

            return Mathf.Max(0f, meter - rate * Time.deltaTime);
        }

        private void TickDrain()
        {
            var emptyMeterCount = EmptyMeterCount;
            if (emptyMeterCount == 0 || _playerHealth == null)
            {
                _drainTimeRemaining = drainIntervalSeconds;
                return;
            }

            _drainTimeRemaining -= Time.deltaTime;
            if (_drainTimeRemaining > 0f)
            {
                return;
            }

            _drainTimeRemaining = drainIntervalSeconds;
            _playerHealth.TakeDamage(drainAmountPerTick * emptyMeterCount);
        }

        public void RestoreHunger(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _hunger = Mathf.Min(MaxMeter, _hunger + amount);
            HungerChanged?.Invoke(_hunger, MaxMeter);
        }

        public void RestoreThirst(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _thirst = Mathf.Min(MaxMeter, _thirst + amount);
            ThirstChanged?.Invoke(_thirst, MaxMeter);
        }

        // Sets both meters directly - used by SaveSystem when loading a save.
        public void LoadState(float savedHunger, float savedThirst)
        {
            _hunger = Mathf.Clamp(savedHunger, 0f, MaxMeter);
            _thirst = Mathf.Clamp(savedThirst, 0f, MaxMeter);
            HungerChanged?.Invoke(_hunger, MaxMeter);
            ThirstChanged?.Invoke(_thirst, MaxMeter);
        }
    }
}
