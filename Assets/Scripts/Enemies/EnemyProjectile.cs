using CIS2991Project.Core;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Spawned by Enemy.FireProjectile. A standalone component, not an implementation detail of
    // Enemy, even though Enemy is the only thing that creates one.
    public sealed class EnemyProjectile : MonoBehaviour, IProjectile
    {
        private int _damage;
        private float _remaining;

        public void Initialize(int damage, float lifetime)
        {
            _damage = Mathf.Max(1, damage);
            _remaining = lifetime;
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Covers both other enemy projectiles and player projectiles (PlayerProjectile also
            // implements IProjectile) - projectiles shouldn't collide with each other.
            if (other.GetComponent<IProjectile>() != null || other.GetComponent<Enemy>() != null)
            {
                return;
            }

            var player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeCombatDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}
