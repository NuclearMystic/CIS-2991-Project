using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Items
{
    public class HealthZone : MonoBehaviour
    {
        public enum EffectType { Damage, Heal }

        [SerializeField] private EffectType effectType = EffectType.Damage;
        [SerializeField, Min(1)] private int amount = 10;
        [SerializeField, Min(0f)] private float cooldown = 1f;

        private float _nextTriggerTime;

        private void OnTriggerEnter2D(Collider2D other)
        {
            ApplyEffect(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (cooldown <= 0f || Time.time >= _nextTriggerTime)
            {
                ApplyEffect(other);
            }
        }

        private void ApplyEffect(Collider2D other)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health == null)
            {
                return;
            }

            if (effectType == EffectType.Damage)
            {
                health.TakeDamage(amount);
            }
            else
            {
                health.Heal(amount);
            }

            _nextTriggerTime = Time.time + cooldown;
        }
    }
}
