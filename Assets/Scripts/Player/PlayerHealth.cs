using System;
using UnityEngine;

namespace CIS2991Project.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;

        public event Action<int, int> HealthChanged;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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
