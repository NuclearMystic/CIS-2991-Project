using System;
using System.Collections.Generic;
using CIS2991Project.Core;
using UnityEngine;

namespace CIS2991Project.Player
{
    // Animator parameters this drives (must exist on the player's own AnimatorController):
    //   Direction (int)     0=Down, 1=Up, 2=Left, 3=Right - same convention as Enemy.cs.
    //   IsMoving  (bool)    true while the movement input is non-zero.
    //   IsDead    (bool)    true once health hits 0.
    //   Died      (trigger) fired once, at the moment of death.
    // Attack/swing visuals are handled entirely by PlayerWeaponVisual's per-weapon frame arrays, not
    // by this Animator - the body itself has no attack state.
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
        private PlayerWeaponVisual _weaponVisual;
        private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrideBuffer = new();
        private Direction _facing = Direction.Down;
        private bool _isDead;
        private bool _equipmentArmed;
        private bool? _appliedArmedPose;

        public Direction Facing => _facing;

        // Sets facing directly and pushes it to the Animator immediately - used by SaveSystem when
        // loading a save.
        public void LoadState(Direction savedFacing)
        {
            _facing = savedFacing;
            _animator.SetInteger("Direction", (int)_facing);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _health = GetComponent<PlayerHealth>();
            _inventory = GetComponent<PlayerInventory>();
            _weaponVisual = GetComponentInChildren<PlayerWeaponVisual>();

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

            // Without this, facing direction (and therefore the equipped weapon's sprite, which reads
            // this Animator's "Direction" param) keeps updating from raw WASD input every frame while
            // paused.
            if (Time.timeScale == 0f)
                return;

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var isMoving = input != Vector2.zero;
            if (isMoving && !Input.GetButton("Fire2"))
                _facing = DetermineFacing(input);

            _animator.SetInteger("Direction", (int)_facing);
            _animator.SetBool("IsMoving", isMoving);

            RefreshArmedPose();
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
            var weapon = _inventory != null ? _inventory.EquippedWeapon : null;
            // Fists are always "equipped" as the no-weapon fallback (see PlayerInventory), but the body
            // should still look unarmed while holding nothing but its own hands - except while they're
            // mid-swing, handled by RefreshArmedPose below.
            _equipmentArmed = weapon != null && weapon != _inventory.FistsItem;
            RefreshArmedPose();
        }

        private void RefreshArmedPose()
        {
            // The unarmed idle/run clips already draw their own idle arms, which doubles up visually
            // with the fists' punch overlay frames - borrow the armed pose for the duration of the swing.
            var isSwingingFists = !_equipmentArmed && _weaponVisual != null && _weaponVisual.IsAttacking;
            var isArmed = _equipmentArmed || isSwingingFists;

            if (isArmed == _appliedArmedPose)
                return;

            _appliedArmedPose = isArmed;
            ApplyWeaponClips(isArmed);
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

        private static Direction DetermineFacing(Vector2 input)
        {
            return Mathf.Abs(input.y) > Mathf.Abs(input.x)
                ? (input.y > 0f ? Direction.Up : Direction.Down)
                : (input.x < 0f ? Direction.Left : Direction.Right);
        }
    }
}
