using System.Collections;
using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Quick color flash + shake played when a hit is confirmed. Needs a MonoBehaviour to host its
    // coroutine (StartCoroutine isn't available outside one), so Enemy passes itself in.
    public sealed class EnemyHitFeedback
    {
        private readonly MonoBehaviour _host;
        private readonly SpriteRenderer _spriteRenderer;
        private readonly Color _originalColor;
        private readonly Color _flashColor;
        private readonly float _flashDuration;
        private readonly float _shakeMagnitude;

        private Vector3 _shakeOffset = Vector3.zero;
        private Coroutine _routine;

        public EnemyHitFeedback(MonoBehaviour host, SpriteRenderer spriteRenderer, Color flashColor, float flashDuration, float shakeMagnitude)
        {
            _host = host;
            _spriteRenderer = spriteRenderer;
            _originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            _flashColor = flashColor;
            _flashDuration = flashDuration;
            _shakeMagnitude = shakeMagnitude;
        }

        public void Play()
        {
            if (_spriteRenderer == null)
                return;

            if (_routine != null)
            {
                _host.StopCoroutine(_routine);
                _host.transform.position -= _shakeOffset;
                _shakeOffset = Vector3.zero;
            }

            _routine = _host.StartCoroutine(Routine());
        }

        // Shakes as an additive offset that's undone and reapplied each frame, rather than driving
        // transform.position directly - the same transform is also moved by physics every
        // FixedUpdate (via Enemy's Rigidbody2D velocity), so a naive shake would fight that and
        // either drift the enemy off its real position or snap back to a stale one once it ends.
        private IEnumerator Routine()
        {
            _spriteRenderer.color = _flashColor;

            var elapsed = 0f;
            while (elapsed < _flashDuration)
            {
                _host.transform.position -= _shakeOffset;
                _shakeOffset = (Vector3)Random.insideUnitCircle * _shakeMagnitude;
                _host.transform.position += _shakeOffset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            _host.transform.position -= _shakeOffset;
            _shakeOffset = Vector3.zero;
            _spriteRenderer.color = _originalColor;
            _routine = null;
        }
    }
}
