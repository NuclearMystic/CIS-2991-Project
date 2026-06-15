using CIS2991Project.Enemies;
using UnityEngine;

namespace CIS2991Project.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        private Vector2 _lastDirection = Vector2.right;
        private float _cooldown;
        private const float FireCooldown = 0.2f;

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input != Vector2.zero)
                _lastDirection = input.normalized;

            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Space) && _cooldown <= 0f)
            {
                SpawnProjectile();
                _cooldown = FireCooldown;
            }
        }

        private void SpawnProjectile()
        {
            var go = new GameObject("Projectile");
            go.transform.position = (Vector2)transform.position + _lastDirection * 0.6f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(Color.white);
            sr.sortingOrder = 2;
            go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            go.AddComponent<Projectile>().Initialize(_lastDirection);
        }

        private static Sprite CreateCircleSprite(Color color)
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var center = (size - 1) / 2f;
            var radius = size / 2f - 0.5f;

            for (var i = 0; i < pixels.Length; i++)
            {
                float x = i % size;
                float y = i / size;
                pixels[i] = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) <= radius
                    ? (Color32)color
                    : new Color32(0, 0, 0, 0);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private class Projectile : MonoBehaviour
        {
            private const float Speed = 12f;
            private const float Lifetime = 4f;

            private float _remaining;

            public void Initialize(Vector2 direction)
            {
                _remaining = Lifetime;
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
                var enemy = other.GetComponent<DemoEnemy>();
                if (enemy != null)
                {
                    enemy.TakeHit(GetComponent<Rigidbody2D>().linearVelocity.normalized);
                    Destroy(gameObject);
                    return;
                }

                if (other.GetComponentInParent<PlayerHealth>() == null)
                    Destroy(gameObject);
            }
        }
    }
}
