using CIS2991Project.Core;
using CIS2991Project.Enemies;
using CIS2991Project.Levels;
using UnityEngine;

namespace CIS2991Project.Player
{
    // Spawned by PlayerShoot.SpawnPellet. A standalone component, not an implementation detail of
    // PlayerShoot, even though PlayerShoot is the only thing that creates one.
    public sealed class PlayerProjectile : MonoBehaviour, IProjectile
    {
        private const float Speed = 24f;

        private float _remaining;
        private int _damage;
        private AudioClip _hitSound;
        private PlayerShoot _owner;

        public void Initialize(Vector2 direction, int damage, float lifetime, AudioClip hitSound, PlayerShoot owner)
        {
            _remaining = lifetime;
            _damage = Mathf.Max(1, damage);
            _hitSound = hitSound;
            _owner = owner;
            GetComponent<Rigidbody2D>().linearVelocity = direction * Speed;
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Covers both other player projectiles and enemy projectiles (EnemyProjectile also
            // implements IProjectile) - projectiles shouldn't collide with each other.
            if (other.GetComponent<IProjectile>() != null)
                return;

            // LevelBounds spans the whole level and the player always stands inside it, so
            // projectiles spawn already overlapping it - it isn't a wall, ignore it.
            if (other.GetComponent<LevelBounds>() != null)
                return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeHit(GetComponent<Rigidbody2D>().linearVelocity.normalized, _damage);
                _owner?.PlaySfx(_hitSound, transform.position);
                Destroy(gameObject);
                return;
            }

            if (other.GetComponentInParent<PlayerHealth>() == null)
                Destroy(gameObject);
        }
    }
}
