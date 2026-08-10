using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerWeaponVisual : MonoBehaviour
    {
        [Header("Position offset per direction (added to this object's starting local position)")]
        [SerializeField] private Vector2 downOffset;
        [SerializeField] private Vector2 upOffset;
        [SerializeField] private Vector2 leftOffset;
        [SerializeField] private Vector2 rightOffset;

        [Header("Sorting order per direction")]
        [SerializeField] private int downSortingOrder = 3;
        [SerializeField] private int upSortingOrder = 1;
        [SerializeField] private int leftSortingOrder = 3;
        [SerializeField] private int rightSortingOrder = 3;

        private Animator _animator;
        private PlayerInventory _inventory;
        private SpriteRenderer _renderer;
        private Vector3 _basePosition;

        private void Awake()
        {
            _animator = GetComponentInParent<Animator>();
            _inventory = GetComponentInParent<PlayerInventory>();
            _renderer = GetComponent<SpriteRenderer>();
            _basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            var weapon = _inventory != null ? _inventory.EquippedWeapon : null;
            if (weapon == null)
            {
                _renderer.enabled = false;
                return;
            }

            var direction = _animator.GetInteger("Direction");
            _renderer.sprite = GetDirectionalSprite(weapon, direction);
            _renderer.enabled = _renderer.sprite != null;
            _renderer.sortingOrder = GetSortingOrder(direction);
            transform.localPosition = _basePosition + (Vector3)GetOffset(direction);
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

        private Vector2 GetOffset(int direction)
        {
            return direction switch
            {
                1 => upOffset,
                2 => leftOffset,
                3 => rightOffset,
                _ => downOffset,
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
