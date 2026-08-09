using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerWeaponVisual : MonoBehaviour
    {
        private Animator _animator;
        private PlayerInventory _inventory;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _animator = GetComponentInParent<Animator>();
            _inventory = GetComponentInParent<PlayerInventory>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            var weapon = _inventory != null ? _inventory.EquippedWeapon : null;
            if (weapon == null)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.sprite = GetDirectionalSprite(weapon, _animator.GetInteger("Direction"));
            _renderer.enabled = _renderer.sprite != null;
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
    }
}
