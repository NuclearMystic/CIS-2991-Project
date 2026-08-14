using System.Collections;
using CIS2991Project.Player;
using CIS2991Project.Items;
using CIS2991Project.UI;
using UnityEngine;

namespace CIS2991Project.Enemies
{
    public class DemoEnemy : MonoBehaviour
    {
        public static int ActiveZombieCount { get; private set; }
        public static event System.Action ZombieKilled;
        private SpriteRenderer _sr;
        private Rigidbody2D _rb;
        private PlayerHealth _player;

        private int _hp = 3;
        private float _patrolSpeed = 2.0f;
        private float _chaseSpeed = 3.2f;
        private global::Item[] _drops;
        private Sprite _dropSprite;
        private Sprite _attackSheet;
        private Sprite _impactOneSheet;
        private Sprite _impactTwoSheet;
        private bool _isDead;
        private bool _isKnockedBack;
        private float _damageCooldown;
        private int _expReward = 15;
        [SerializeField, Min(0)] private int capsReward = 10;
        private CharacterSheet _characterSheet;

        private Vector2 _patrolStart;
        private Vector2 _patrolEnd;
        private Vector2 _patrolTarget;

        private const float PatrolSpeed = 2f;
        private const float ChaseSpeed = 3.2f;
        private const float ChaseRange = 10f;
        private const float ContactRange = 1.1f;
        private const float DamageCooldown = 0.5f;

        public static void Spawn(Vector2 patrolStart, Vector2 patrolEnd)
        {
            var go = new GameObject("DemoEnemy");
            go.transform.position = patrolStart;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSprite(Color.red);
            go.transform.localScale = Vector3.one;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<BoxCollider2D>();

            go.AddComponent<DemoEnemy>().Init(patrolStart, patrolEnd);
        }

        public static void Spawn(Vector2 patrolStart, Vector2 patrolEnd, Sprite visual, Sprite dropVisual, global::Item[] drops, int hitPoints, float speedMultiplier,
            Sprite attackSheet = null, Sprite impactOneSheet = null, Sprite impactTwoSheet = null)
        {
            var go = new GameObject("Zombie");
            go.transform.position = patrolStart;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = visual != null ? visual : CreateSprite(Color.red);
            sr.sortingOrder = 4;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<BoxCollider2D>();

            go.AddComponent<DemoEnemy>().Init(patrolStart, patrolEnd, hitPoints, speedMultiplier, drops, dropVisual, attackSheet, impactOneSheet, impactTwoSheet);
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            ActiveZombieCount++;
        }

        private void OnDestroy()
        {
            ActiveZombieCount = Mathf.Max(0, ActiveZombieCount - 1);
        }

        private void Start()
        {
            _player = Object.FindAnyObjectByType<PlayerHealth>();
            _characterSheet = _player != null ? _player.GetComponent<CharacterSheet>() : null;
        }

        public void Init(Vector2 patrolStart, Vector2 patrolEnd)
        {
            _patrolStart = patrolStart;
            _patrolEnd = patrolEnd;
            _patrolTarget = patrolEnd;
        }

        public void Init(Vector2 patrolStart, Vector2 patrolEnd, int hitPoints, float speedMultiplier, global::Item[] drops, Sprite dropVisual,
            Sprite attackSheet = null, Sprite impactOneSheet = null, Sprite impactTwoSheet = null)
        {
            Init(patrolStart, patrolEnd);
            _hp = Mathf.Max(1, hitPoints);
            _patrolSpeed = PatrolSpeed * Mathf.Max(.5f, speedMultiplier);
            _chaseSpeed = ChaseSpeed * Mathf.Max(.5f, speedMultiplier);
            _expReward = Mathf.Max(5, _hp * 5);
            _drops = drops;
            _dropSprite = dropVisual;
            _attackSheet = attackSheet;
            _impactOneSheet = impactOneSheet;
            _impactTwoSheet = impactTwoSheet;
        }

        private void Update()
        {
            if (_isDead || _isKnockedBack || _player == null) return;

            if (_damageCooldown > 0f)
                _damageCooldown -= Time.deltaTime;

            var pos = (Vector2)transform.position;
            var playerPos = (Vector2)_player.transform.position;
            var distToPlayer = Vector2.Distance(pos, playerPos);

            Vector2 moveDir;
            float speed;

            if (distToPlayer <= ChaseRange)
            {
                moveDir = (playerPos - pos).normalized;
                speed = _chaseSpeed;

                if (distToPlayer <= ContactRange && _damageCooldown <= 0f)
                {
                    _player.TakeDamage(1);
                    PlayAttackFeedback();
                    _damageCooldown = DamageCooldown;
                }
            }
            else
            {
                var toTarget = _patrolTarget - pos;
                if (toTarget.magnitude < 0.1f)
                    _patrolTarget = _patrolTarget == _patrolEnd ? _patrolStart : _patrolEnd;
                moveDir = toTarget.normalized;
                speed = _patrolSpeed;
            }

            _rb.MovePosition(pos + moveDir * speed * Time.deltaTime);
        }

        public void TakeHit(Vector2 hitDirection, int damage = 1)
        {
            if (_isDead) return;
            _hp -= Mathf.Max(1, damage);
            if (_hp <= 0)
            {
                _isDead = true;
                DropLoot();
                _characterSheet?.AddExperience(_expReward);
                _player?.GetComponent<PlayerInventory>()?.AddCurrency(capsReward);
                ZombieKilled?.Invoke();
                Destroy(gameObject);
                return;
            }
            StartCoroutine(HitRoutine(hitDirection));
        }

        private void DropLoot()
        {
            if (_drops == null || _drops.Length == 0)
            {
                return;
            }

            var item = _drops[Random.Range(0, _drops.Length)];
            if (item == null)
            {
                return;
            }

            var loot = new GameObject($"Loot_{item.displayName}");
            loot.transform.position = transform.position;
            var renderer = loot.AddComponent<SpriteRenderer>();
            renderer.sprite = _dropSprite != null ? _dropSprite : CreateSprite(Color.yellow);
            renderer.sortingOrder = 5;
            loot.AddComponent<CircleCollider2D>().isTrigger = true;
            loot.AddComponent<ConsumablePickup>().Configure(item, 1);
        }

        private void PlayAttackFeedback()
        {
            if (_attackSheet != null)
            {
                var animator = GetComponent<SpriteStripAnimator>() ?? gameObject.AddComponent<SpriteStripAnimator>();
                animator.Play(_attackSheet, 7, 14f, false);
            }

            var impactSheet = Random.value < .5f ? _impactOneSheet : _impactTwoSheet;
            if (impactSheet == null || _player == null)
                return;

            var effect = new GameObject("Zombie Hit Effect");
            effect.transform.position = _player.transform.position;
            effect.transform.localScale = Vector3.one * 1.5f;
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 8;
            var animatorEffect = effect.AddComponent<SpriteStripAnimator>();
            animatorEffect.Play(impactSheet, 3, 18f, false);
            Destroy(effect, .2f);
        }

        private IEnumerator HitRoutine(Vector2 hitDirection)
        {
            _isKnockedBack = true;
            _sr.color = Color.black;

            var startPos = (Vector2)transform.position;
            var endPos = startPos + hitDirection;
            var elapsed = 0f;
            const float duration = 0.12f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _rb.MovePosition(Vector2.Lerp(startPos, endPos, elapsed / duration));
                yield return null;
            }

            _rb.MovePosition(endPos);
            yield return new WaitForSeconds(0.1f);
            _sr.color = Color.red;
            _isKnockedBack = false;
        }

        private static Sprite CreateSprite(Color color)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color32[16 * 16];
            var c = (Color32)color;
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = c;
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
