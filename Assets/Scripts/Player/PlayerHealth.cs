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

        public event Action<int, int> HealthChanged;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            _characterSheet = GetComponent<CharacterSheet>();
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

        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(currentHealth - amount, 0);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
