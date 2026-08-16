using CIS2991Project.Core;
using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerWeaponVisual : MonoBehaviour
    {
        // Bundles the four per-direction values (idle sprite, attack frames, offset, sorting order) so
        // LateUpdate needs one switch instead of four separate ones to look them up.
        private readonly struct DirectionalVisual
        {
            public readonly Sprite IdleSprite;
            public readonly Sprite[] AttackFrames;
            public readonly Vector2 Offset;
            public readonly int SortingOrder;

            public DirectionalVisual(Sprite idleSprite, Sprite[] attackFrames, Vector2 offset, int sortingOrder)
            {
                IdleSprite = idleSprite;
                AttackFrames = attackFrames;
                Offset = offset;
                SortingOrder = sortingOrder;
            }
        }

        [Header("Sorting order per direction")]
        [SerializeField] private int downSortingOrder = 3;
        [SerializeField] private int upSortingOrder = 1;
        [SerializeField] private int leftSortingOrder = 3;
        [SerializeField] private int rightSortingOrder = 3;

        private Animator _animator;
        private PlayerInventory _inventory;
        private PlayerShoot _shoot;
        private SpriteRenderer _renderer;
        private Vector3 _basePosition;

        private bool _isAttacking;
        private int _attackFrameIndex;
        private float _attackFrameTimer;

        // Lets PlayerAnimationController temporarily switch the body to its armed pose while fists
        // are mid-swing - the unarmed pose already draws its own idle arms, which doubles up with the
        // punch overlay frames.
        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _animator = GetComponentInParent<Animator>();
            _inventory = GetComponentInParent<PlayerInventory>();
            _shoot = GetComponentInParent<PlayerShoot>();
            _renderer = GetComponent<SpriteRenderer>();
            _basePosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (_shoot == null)
                return;

            _shoot.Fired += HandleFired;
            _shoot.MeleeAttacked += HandleAttackStarted;
        }

        private void OnDisable()
        {
            if (_shoot == null)
                return;

            _shoot.Fired -= HandleFired;
            _shoot.MeleeAttacked -= HandleAttackStarted;
        }

        private void HandleFired(Vector2 direction) => HandleAttackStarted();

        private void HandleAttackStarted()
        {
            _isAttacking = true;
            _attackFrameIndex = 0;
            _attackFrameTimer = 0f;
        }

        private void LateUpdate()
        {
            var weapon = _inventory != null ? _inventory.EquippedWeapon : null;
            if (weapon == null)
            {
                _renderer.enabled = false;
                _isAttacking = false;
                return;
            }

            var direction = (Direction)_animator.GetInteger("Direction");
            var visual = GetDirectionalVisual(weapon, direction);
            _renderer.sprite = _isAttacking
                ? AdvanceAttackPlayback(visual.AttackFrames, visual.IdleSprite, weapon.attackFrameRate)
                : visual.IdleSprite;

            _renderer.enabled = _renderer.sprite != null;
            _renderer.sortingOrder = visual.SortingOrder;
            transform.localPosition = _basePosition + (Vector3)visual.Offset;
        }

        // Steps through the equipped weapon's attack frames for the given direction, falling back to
        // the idle sprite once the sequence finishes (or immediately, if this weapon has no frames set).
        private Sprite AdvanceAttackPlayback(Sprite[] frames, Sprite idleSprite, float attackFrameRate)
        {
            if (frames == null || frames.Length == 0)
            {
                _isAttacking = false;
                return idleSprite;
            }

            var frame = frames[Mathf.Min(_attackFrameIndex, frames.Length - 1)];

            var secondsPerFrame = 1f / Mathf.Max(1f, attackFrameRate);
            _attackFrameTimer += Time.deltaTime;
            if (_attackFrameTimer >= secondsPerFrame)
            {
                _attackFrameTimer -= secondsPerFrame;
                _attackFrameIndex++;
                if (_attackFrameIndex >= frames.Length)
                    _isAttacking = false;
            }

            return frame;
        }

        private DirectionalVisual GetDirectionalVisual(global::Item weapon, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new DirectionalVisual(weapon.equippedSpriteUp, weapon.attackFramesUp, weapon.equippedOffsetUp, upSortingOrder),
                Direction.Left => new DirectionalVisual(weapon.equippedSpriteLeft, weapon.attackFramesLeft, weapon.equippedOffsetLeft, leftSortingOrder),
                Direction.Right => new DirectionalVisual(weapon.equippedSpriteRight, weapon.attackFramesRight, weapon.equippedOffsetRight, rightSortingOrder),
                _ => new DirectionalVisual(weapon.equippedSpriteDown, weapon.attackFramesDown, weapon.equippedOffsetDown, downSortingOrder),
            };
        }
    }
}
