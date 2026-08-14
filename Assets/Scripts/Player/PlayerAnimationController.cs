using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [Serializable]
        private struct DirectionalClips
        {
            public AnimationClip down;
            public AnimationClip up;
            public AnimationClip left;
            public AnimationClip right;
        }

        private enum FacingDirection { Down, Up, Left, Right }

        [Header("Must match the clips already wired into the base controller's Idle states")]
        [SerializeField] private DirectionalClips unarmedIdle;
        [SerializeField] private DirectionalClips armedIdle;

        [Header("Must match the clips already wired into the base controller's Run states")]
        [SerializeField] private DirectionalClips unarmedRun;
        [SerializeField] private DirectionalClips armedRun;

        private Animator _animator;
        private AnimatorOverrideController _overrideController;
        private PlayerHealth _health;
        private PlayerInventory _inventory;
        private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrideBuffer = new();
        private FacingDirection _facing = FacingDirection.Down;
        private bool _isDead;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _health = GetComponent<PlayerHealth>();
            _inventory = GetComponent<PlayerInventory>();

            _overrideController = _animator.runtimeAnimatorController as AnimatorOverrideController;
            if (_overrideController == null)
                Debug.LogWarning("PlayerAnimationController: Animator's Controller must be an AnimatorOverrideController for equip/unequip clip swapping to work.");
        }

        private void OnEnable()
        {
            if (_health != null) _health.HealthChanged += HandleHealthChanged;
            if (_inventory != null) _inventory.EquipmentChanged += HandleEquipmentChanged;
            HandleEquipmentChanged();
        }

        private void OnDisable()
        {
            if (_health != null) _health.HealthChanged -= HandleHealthChanged;
            if (_inventory != null) _inventory.EquipmentChanged -= HandleEquipmentChanged;
        }

        private void Update()
        {
            if (_isDead)
                return;

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var isMoving = input != Vector2.zero;
            if (isMoving && !Input.GetButton("Fire2"))
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

        private void HandleEquipmentChanged()
        {
            ApplyWeaponClips(_inventory != null && _inventory.EquippedWeapon != null);
        }

        private void ApplyWeaponClips(bool isArmed)
        {
            if (_overrideController == null)
                return;

            var idle = isArmed ? armedIdle : unarmedIdle;
            var run = isArmed ? armedRun : unarmedRun;

            _overrideBuffer.Clear();
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedIdle.down, idle.down));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedIdle.up, idle.up));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedIdle.left, idle.left));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedIdle.right, idle.right));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedRun.down, run.down));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedRun.up, run.up));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedRun.left, run.left));
            _overrideBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(unarmedRun.right, run.right));
            _overrideController.ApplyOverrides(_overrideBuffer);
        }

        private static FacingDirection DetermineFacing(Vector2 input)
        {
            return Mathf.Abs(input.y) > Mathf.Abs(input.x)
                ? (input.y > 0f ? FacingDirection.Up : FacingDirection.Down)
                : (input.x < 0f ? FacingDirection.Left : FacingDirection.Right);
        }
    }
}
