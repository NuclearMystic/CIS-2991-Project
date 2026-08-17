using CIS2991Project.Core;
using CIS2991Project.Jobs;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Drop this on any NPC GameObject that has a SpriteRenderer, an Animator (driven by the
    // parameters documented below), and a Collider2D (BoxCollider2D/CapsuleCollider2D - not added
    // automatically since Collider2D is abstract). Assign an EnemyDefinition asset for stats.
    // Patrols within a radius of wherever it starts, chases once the player is within sight range,
    // and stops to attack (melee or ranged, per the definition) once close enough.
    //
    // Animator parameters this drives (must exist on the NPC's own AnimatorController):
    //   Direction (int)     0=Down, 1=Up, 2=Left, 3=Right - same convention as the player.
    //   IsMoving  (bool)    true while patrolling or chasing, false while idle/attacking.
    //   IsRunning (bool)    true while chasing (alerted), false while patrolling.
    //   Attack    (trigger) fired the instant an attack lands - wire to the shoot or melee-swing clip.
    //   IsDead    (bool)    true once HP hits 0.
    //   Died      (trigger) fired once, at the moment of death.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        private enum State { Patrol, Chase, Attack, Dead }

        private const string DirectionParam = "Direction";
        private const string IsMovingParam = "IsMoving";
        private const string IsRunningParam = "IsRunning";
        private const string IsAttackingParam = "IsAttacking";
        private const string AttackTrigger = "Attack";
        private const string IsDeadParam = "IsDead";
        private const string DiedTrigger = "Died";

        [SerializeField] private EnemyDefinition definition;

        [Header("Patrol — wanders within this radius of wherever it starts")]
        [SerializeField, Min(0f)] private float patrolRadius = 3f;
        [SerializeField, Min(0f)] private float patrolPauseSeconds = 1.5f;

        [SerializeField, Min(0f)] private float deathAnimationSeconds = 1f;

        [Header("Hit Feedback — quick red flash + shake when damage is confirmed")]
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField, Min(0f)] private float hitFlashDuration = 0.15f;
        [SerializeField, Min(0f)] private float hitShakeMagnitude = 0.06f;

        private Rigidbody2D _body;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private PlayerHealth _player;
        private CharacterSheet _playerCharacterSheet;
        private EnemyHitFeedback _hitFeedback;

        private State _state = State.Patrol;
        private Vector2 _homePosition;
        private Vector2 _patrolTarget;
        private Vector2 _facingDirection;
        private float _patrolPauseRemaining;
        private float _attackCooldownRemaining;
        private int _currentHealth;

        public float PatrolRadius => patrolRadius;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;

            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _hitFeedback = new EnemyHitFeedback(this, _spriteRenderer, hitFlashColor, hitFlashDuration, hitShakeMagnitude);

            _currentHealth = definition != null ? definition.maxHealth : 1;
        }

        private void Start()
        {
            _homePosition = transform.position;
            _patrolTarget = _homePosition;
            _facingDirection = Vector2.down;

            _player = Object.FindAnyObjectByType<PlayerHealth>();
            _playerCharacterSheet = _player != null ? _player.GetComponent<CharacterSheet>() : null;
        }

        private void Update()
        {
            if (_state == State.Dead || definition == null || _player == null)
            {
                return;
            }

            if (_attackCooldownRemaining > 0f)
            {
                _attackCooldownRemaining -= Time.deltaTime;
            }

            var distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

            _state = distanceToPlayer <= definition.attackRange
                ? State.Attack
                : distanceToPlayer <= definition.sightRange
                    ? State.Chase
                    : State.Patrol;

            switch (_state)
            {
                case State.Patrol:
                    UpdatePatrol();
                    break;
                case State.Chase:
                    UpdateChase();
                    break;
                case State.Attack:
                    UpdateAttack();
                    break;
            }

            UpdateAnimator();
        }

        private void UpdatePatrol()
        {
            var position = (Vector2)transform.position;
            var toTarget = _patrolTarget - position;

            if (toTarget.magnitude < 0.15f)
            {
                _facingDirection = Vector2.zero;
                _body.linearVelocity = Vector2.zero;

                if (_patrolPauseRemaining > 0f)
                {
                    _patrolPauseRemaining -= Time.deltaTime;
                    return;
                }

                _patrolTarget = _homePosition + Random.insideUnitCircle * patrolRadius;
                _patrolPauseRemaining = patrolPauseSeconds;
                return;
            }

            _facingDirection = toTarget.normalized;
            _body.linearVelocity = _facingDirection * definition.patrolSpeed;
        }

        private void UpdateChase()
        {
            var position = (Vector2)transform.position;
            _facingDirection = ((Vector2)_player.transform.position - position).normalized;
            _body.linearVelocity = _facingDirection * definition.chaseSpeed;
        }

        private void UpdateAttack()
        {
            _body.linearVelocity = Vector2.zero;
            _facingDirection = ((Vector2)_player.transform.position - (Vector2)transform.position).normalized;

            if (_attackCooldownRemaining > 0f)
            {
                return;
            }

            _attackCooldownRemaining = definition.attackCooldown;
            _animator?.SetTrigger(AttackTrigger);

            if (definition.attackType == EnemyAttackType.Ranged)
            {
                FireProjectile();
            }
            else
            {
                _player.TakeDamage(definition.attackDamage);
            }
        }

        private void FireProjectile()
        {
            var muzzlePosition = (Vector2)transform.position;
            var direction = ((Vector2)_player.transform.position - muzzlePosition).normalized;

            var go = new GameObject("EnemyProjectile");
            go.transform.position = muzzlePosition + direction * 0.5f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = definition.projectileSprite != null ? definition.projectileSprite : RuntimeSpriteUtils.CreateCircleSprite(Color.red);
            sr.sortingOrder = 6;
            if (definition.projectileSprite == null)
            {
                go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearVelocity = direction * definition.projectileSpeed;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            go.AddComponent<EnemyProjectile>().Initialize(definition.attackDamage, definition.projectileLifetime);
        }

        private void UpdateAnimator()
        {
            if (_animator == null)
            {
                return;
            }

            var isMoving = (_state == State.Patrol || _state == State.Chase) && _facingDirection != Vector2.zero;
            _animator.SetBool(IsMovingParam, isMoving);
            _animator.SetBool(IsRunningParam, _state == State.Chase);
            _animator.SetBool(IsAttackingParam, _state == State.Attack);

            if (_facingDirection != Vector2.zero)
            {
                _animator.SetInteger(DirectionParam, (int)DetermineFacing(_facingDirection));
            }
        }

        public void TakeHit(Vector2 hitDirection, int damage)
        {
            if (_state == State.Dead || definition == null)
            {
                return;
            }

            _hitFeedback.Play();

            _currentHealth -= Mathf.Max(1, damage);
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _state = State.Dead;
            _body.linearVelocity = Vector2.zero;
            JobManager.ReportKill(definition.displayName);

            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            if (_animator != null)
            {
                _animator.SetBool(IsDeadParam, true);
                _animator.SetTrigger(DiedTrigger);
            }

            Invoke(nameof(DropLoot), deathAnimationSeconds);
            _playerCharacterSheet?.AddExperience(definition.expReward);

            Destroy(gameObject, deathAnimationSeconds);
        }

        // Invoke() needs this as a same-instance named method - the actual loot logic lives in
        // EnemyLoot.Spawn, shared with anything else that wants to roll a loot table.
        private void DropLoot()
        {
            EnemyLoot.Spawn(definition, transform.position);
        }

        private static Direction DetermineFacing(Vector2 direction)
        {
            return Mathf.Abs(direction.y) > Mathf.Abs(direction.x)
                ? (direction.y > 0f ? Direction.Up : Direction.Down)
                : (direction.x < 0f ? Direction.Left : Direction.Right);
        }
    }
}
