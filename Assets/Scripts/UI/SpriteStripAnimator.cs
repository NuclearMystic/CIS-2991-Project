using UnityEngine;

namespace CIS2991Project.UI
{
    /// <summary>Plays a horizontal sprite-sheet strip without requiring animation assets.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteStripAnimator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _frames;
        private float _framesPerSecond;
        private bool _loop;
        private float _elapsed;

        public void Play(Sprite sheet, int frameCount, float framesPerSecond, bool loop = true)
        {
            if (sheet == null || frameCount <= 0)
                return;

            _renderer ??= GetComponent<SpriteRenderer>();
            _frames = BuildFrames(sheet, frameCount);
            _framesPerSecond = Mathf.Max(1f, framesPerSecond);
            _loop = loop;
            _elapsed = 0f;
            _renderer.sprite = _frames[0];
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0 || _renderer == null)
                return;

            _elapsed += Time.deltaTime;
            var frame = Mathf.FloorToInt(_elapsed * _framesPerSecond);
            if (_loop)
                frame %= _frames.Length;
            else
                frame = Mathf.Min(frame, _frames.Length - 1);
            _renderer.sprite = _frames[frame];
        }

        private static Sprite[] BuildFrames(Sprite sheet, int frameCount)
        {
            var texture = sheet.texture;
            var frameWidth = texture.width / frameCount;
            var frames = new Sprite[frameCount];
            for (var frame = 0; frame < frameCount; frame++)
            {
                var rect = new Rect(frame * frameWidth, 0f, frameWidth, texture.height);
                frames[frame] = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 20f);
            }
            return frames;
        }
    }
}
