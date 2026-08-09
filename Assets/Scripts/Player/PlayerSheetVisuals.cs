using CIS2991Project.UI;
using UnityEngine;

namespace CIS2991Project.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerSheetVisuals : MonoBehaviour
    {
        private SpriteStripAnimator _animator;
        private PlayerShoot _shooter;
        private PlayerHealth _health;
        private Sprite _idleRight;
        private Sprite _idleLeft;
        private Sprite _shootRight;
        private Sprite _shootDown;
        private Sprite _shotgunIdle;
        private Sprite _reload;
        private Sprite _shotgunReload;
        private Sprite _deathRight;
        private Sprite _deathLeft;
        private float _actionUntil;
        private bool _facingLeft;
        private bool _dead;
        private bool _usingShotgunVisual;

        public void Configure(Sprite idleRight, Sprite idleLeft, Sprite shootRight, Sprite shootDown,
            Sprite shotgunIdle, Sprite reload, Sprite shotgunReload, Sprite deathRight, Sprite deathLeft)
        {
            _idleRight = idleRight;
            _idleLeft = idleLeft;
            _shootRight = shootRight;
            _shootDown = shootDown;
            _shotgunIdle = shotgunIdle;
            _reload = reload;
            _shotgunReload = shotgunReload;
            _deathRight = deathRight;
            _deathLeft = deathLeft;
            _animator ??= GetComponent<SpriteStripAnimator>() ?? gameObject.AddComponent<SpriteStripAnimator>();
            _shooter ??= GetComponent<PlayerShoot>();
            _health ??= GetComponent<PlayerHealth>();
            if (_shooter != null)
                _shooter.Fired -= HandleShot;
            if (_shooter != null)
                _shooter.Fired += HandleShot;
            if (_health != null)
                _health.HealthChanged -= HandleHealthChanged;
            if (_health != null)
                _health.HealthChanged += HandleHealthChanged;
            PlayIdle();
        }

        private void OnDestroy()
        {
            if (_shooter != null) _shooter.Fired -= HandleShot;
            if (_health != null) _health.HealthChanged -= HandleHealthChanged;
        }

        private void Update()
        {
            if (_dead)
                return;

            var horizontal = Input.GetAxisRaw("Horizontal");
            if (horizontal != 0f)
                _facingLeft = horizontal < 0f;

            if (Input.GetKeyDown(KeyCode.Alpha2) && _shotgunIdle != null)
            {
                _usingShotgunVisual = !_usingShotgunVisual;
                _actionUntil = 0f;
                PlayIdle();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                var sheet = _usingShotgunVisual ? _shotgunReload : _reload;
                var frameCount = _usingShotgunVisual ? 9 : 11;
                if (sheet != null)
                {
                    _animator.Play(sheet, frameCount, 12f, false);
                    _actionUntil = Time.time + frameCount / 12f;
                }
            }
            else if (Time.time >= _actionUntil)
            {
                PlayIdle();
            }
        }

        private void HandleShot(Vector2 direction)
        {
            if (_dead) return;
            _facingLeft = direction.x < 0f;
            var sheet = Mathf.Abs(direction.y) > Mathf.Abs(direction.x) ? _shootDown : _shootRight;
            if (sheet != null)
                _animator.Play(sheet, 3, 18f, false);
            _actionUntil = Time.time + .18f;
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            if (currentHealth > 0 || _dead) return;
            _dead = true;
            var sheet = _facingLeft ? _deathLeft : _deathRight;
            if (sheet != null)
                _animator.Play(sheet, 6, 8f, false);
        }

        private void PlayIdle()
        {
            var sheet = _usingShotgunVisual ? _shotgunIdle : (_facingLeft ? _idleLeft : _idleRight);
            if (sheet != null)
                _animator.Play(sheet, 6, 8f);
        }

        public void SelectShotgunStance(bool selected)
        {
            _usingShotgunVisual = selected;
            if (!_dead)
                PlayIdle();
        }
    }
}
