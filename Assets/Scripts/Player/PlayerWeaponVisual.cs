using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerWeaponVisual : MonoBehaviour
    {
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

            var direction = _animator.GetInteger("Direction");
            _renderer.sprite = _isAttacking
                ? AdvanceAttackPlayback(weapon, direction)
                : GetDirectionalSprite(weapon, direction);

            _renderer.enabled = _renderer.sprite != null;
            _renderer.sortingOrder = GetSortingOrder(direction);
            transform.localPosition = _basePosition + (Vector3)GetOffset(weapon, direction);
        }

        // Steps through the equipped weapon's attack frames for the given direction, falling back to
        // the idle sprite once the sequence finishes (or immediately, if this weapon has no frames set).
        private Sprite AdvanceAttackPlayback(global::Item weapon, int direction)
        {
            var frames = GetAttackFrames(weapon, direction);
            if (frames == null || frames.Length == 0)
            {
                _isAttacking = false;
                return GetDirectionalSprite(weapon, direction);
            }

            var frame = frames[Mathf.Min(_attackFrameIndex, frames.Length - 1)];

            var secondsPerFrame = 1f / Mathf.Max(1f, weapon.attackFrameRate);
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

        private static Sprite GetDirectionalSprite(global::Item weapon, int direction)
        {
            return direction switch
            {
                1 => weapon.equippedSpriteUp,
                2 => weapon.equippedSpriteLeft,
                3 => weapon.equippedSpriteRight,
                _ => weapon.equippedSpriteDown,
            };
        }

        private static Sprite[] GetAttackFrames(global::Item weapon, int direction)
        {
            return direction switch
            {
                1 => weapon.attackFramesUp,
                2 => weapon.attackFramesLeft,
                3 => weapon.attackFramesRight,
                _ => weapon.attackFramesDown,
            };
        }

        private static Vector2 GetOffset(global::Item weapon, int direction)
        {
            return direction switch
            {
                1 => weapon.equippedOffsetUp,
                2 => weapon.equippedOffsetLeft,
                3 => weapon.equippedOffsetRight,
                _ => weapon.equippedOffsetDown,
            };
        }

        private int GetSortingOrder(int direction)
        {
            return direction switch
            {
                1 => upSortingOrder,
                2 => leftSortingOrder,
                3 => rightSortingOrder,
                _ => downSortingOrder,
            };
        }
    }
}
