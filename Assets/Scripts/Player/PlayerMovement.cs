using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField, Min(1f)] private float sprintMultiplier = 2f;

        private Rigidbody2D body;
        private CharacterSheet _characterSheet;
        private PlayerShoot _playerShoot;
        private Vector2 movementInput;
        private bool _isSprinting;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            _characterSheet = GetComponent<CharacterSheet>();
            _playerShoot = GetComponent<PlayerShoot>();
        }

        private void Update()
        {
            movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            _isSprinting = Input.GetKey(sprintKey) && (_playerShoot == null || !_playerShoot.IsReloading);
        }

        private void FixedUpdate()
        {
            var speed = moveSpeed * GetAthleticsMultiplier();
            if (_isSprinting)
                speed *= sprintMultiplier;

            body.linearVelocity = movementInput * speed;
        }

        private float GetAthleticsMultiplier()
        {
            return _characterSheet != null ? _characterSheet.GetMoveSpeedMultiplier() : 1f;
        }
    }
}
