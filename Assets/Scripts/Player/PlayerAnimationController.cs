using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        private enum FacingDirection { Down, Up, Left, Right }

        private Animator _animator;
        private PlayerHealth _health;
        private FacingDirection _facing = FacingDirection.Down;
        private bool _isDead;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _health = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            if (_health != null) _health.HealthChanged -= HandleHealthChanged;
        }

        private void Update()
        {
            if (_isDead)
                return;

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var isMoving = input != Vector2.zero;
            if (isMoving)
                _facing = DetermineFacing(input);

            _animator.SetInteger("Direction", (int)_facing);
            _animator.SetBool("IsMoving", isMoving);
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            if (currentHealth > 0 || _isDead) return;
            _isDead = true;
            _animator.SetBool("IsDead", true);
            _animator.SetTrigger("Died");
        }

        private static FacingDirection DetermineFacing(Vector2 input)
        {
            return Mathf.Abs(input.y) > Mathf.Abs(input.x)
                ? (input.y > 0f ? FacingDirection.Up : FacingDirection.Down)
                : (input.x < 0f ? FacingDirection.Left : FacingDirection.Right);
        }
    }
}
