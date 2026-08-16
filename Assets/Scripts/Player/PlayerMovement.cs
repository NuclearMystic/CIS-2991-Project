using CIS2991Project.Managers;
using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField, Min(1f)] private float sprintMultiplier = 2f;

        [Header("Footsteps")]
        [SerializeField] private AudioClip footstepSound;
        [Tooltip("Seconds between footsteps while walking. Scales down with sprintMultiplier while sprinting.")]
        [SerializeField, Min(0.05f)] private float footstepInterval = 0.35f;

        private Rigidbody2D body;
        private AudioSource _audioSource;
        private CharacterSheet _characterSheet;
        private PlayerShoot _playerShoot;
        private Vector2 movementInput;
        private bool _isSprinting;
        private float _footstepTimeRemaining;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            _characterSheet = GetComponent<CharacterSheet>();
            _playerShoot = GetComponent<PlayerShoot>();

            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.outputAudioMixerGroup = AudioManager.SfxGroup;
        }

        private void Update()
        {
            movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            _isSprinting = Input.GetKey(sprintKey) && (_playerShoot == null || !_playerShoot.IsReloading);

            UpdateFootsteps();
        }

        private void UpdateFootsteps()
        {
            if (movementInput == Vector2.zero)
            {
                _footstepTimeRemaining = 0f;
                return;
            }

            _footstepTimeRemaining -= Time.deltaTime;
            if (_footstepTimeRemaining > 0f)
                return;

            _footstepTimeRemaining = _isSprinting ? footstepInterval / sprintMultiplier : footstepInterval;

            if (footstepSound != null)
                _audioSource.PlayOneShot(footstepSound);
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
