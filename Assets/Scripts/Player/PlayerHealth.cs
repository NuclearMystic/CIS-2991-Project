using System;
using UnityEngine;

namespace CIS2991Project.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [Tooltip("Endurance skill drives heart count; each heart is worth this much max health.")]
        [SerializeField, Min(1f)] private float healthPerHeart = 10f;

        private CharacterSheet _characterSheet;
        private PlayerInventory _inventory;

        public event Action<int, int> HealthChanged;

        // Fired with the actual amount applied (after clamping to 0/max), not the raw requested
        // amount - so e.g. healing at full HP or a killing blow past 0 doesn't pop up a misleading
        // number. Only fires when something actually changed.
        public event Action<int> DamageTaken;
        public event Action<int> Healed;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            _characterSheet = GetComponent<CharacterSheet>();
            _inventory = GetComponent<PlayerInventory>();
            if (_characterSheet == null)
            {
                _characterSheet = UnityEngine.Object.FindAnyObjectByType<CharacterSheet>();
            }

            if (_characterSheet != null)
            {
                _characterSheet.SkillChanged += HandleSkillChanged;
            }

            RecalculateMaxHealth(fullHeal: true);
        }

        private void OnDestroy()
        {
            if (_characterSheet != null)
            {
                _characterSheet.SkillChanged -= HandleSkillChanged;
            }
        }

        private void HandleSkillChanged(SkillType skill, int level)
        {
            if (skill == SkillType.Endurance)
            {
                RecalculateMaxHealth(fullHeal: false);
            }
        }

        private void RecalculateMaxHealth(bool fullHeal)
        {
            if (_characterSheet == null)
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                HealthChanged?.Invoke(currentHealth, maxHealth);
                return;
            }

            var previousMax = maxHealth;
            maxHealth = Mathf.RoundToInt(_characterSheet.GetHeartCount() * healthPerHeart);

            currentHealth = fullHeal
                ? maxHealth
                : Mathf.Clamp(currentHealth + (maxHealth - previousMax), 0, maxHealth);

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // Raw damage, no armor mitigation - for non-combat sources (e.g. SurvivalStats' starvation/
        // dehydration drain), where a vest shouldn't help.
        public void TakeDamage(int amount)
        {
            ApplyDamage(amount);
        }

        // Combat hits (Enemy melee/ranged attacks) go through armor mitigation first.
        public void TakeCombatDamage(int amount)
        {
            ApplyDamage(ApplyArmorReduction(amount));
        }

        private void ApplyDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var previousHealth = currentHealth;
            currentHealth = Mathf.Max(currentHealth - amount, 0);

            var actualDamage = previousHealth - currentHealth;
            if (actualDamage > 0)
            {
                DamageTaken?.Invoke(actualDamage);
            }

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // Item.defense is a flat percent (10 = -10% incoming damage), not a per-point formula - see
        // Item.cs. No armor equipped applies no reduction.
        private int ApplyArmorReduction(int amount)
        {
            var armor = _inventory != null ? _inventory.EquippedArmor : null;
            if (armor == null || armor.defense <= 0)
            {
                return amount;
            }

            var reductionFraction = Mathf.Clamp01(armor.defense / 100f);
            return Mathf.RoundToInt(amount * (1f - reductionFraction));
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var previousHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            var actualHeal = currentHealth - previousHealth;
            if (actualHeal > 0)
            {
                Healed?.Invoke(actualHeal);
            }

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // Sets health directly rather than through TakeDamage/Heal's relative math - used by
        // SaveSystem when loading a save. Call after CharacterSheet.LoadState, since that reactively
        // recalculates maxHealth via HandleSkillChanged (Endurance) and this should clamp against the
        // restored max, not whatever it was before.
        public void LoadState(int savedCurrentHealth)
        {
            currentHealth = Mathf.Clamp(savedCurrentHealth, 0, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
