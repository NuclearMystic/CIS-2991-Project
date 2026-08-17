using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Floats "-N" (red, damage) / "+N" (green, healing) numbers above whatever Transform it's told to
    // follow (the player or an Enemy), same rise-and-fade technique as PickupPopupHud - but tracks a
    // list rather than one slot, since multiple hits (or a hit landing close to a heal) can land in
    // the same second and each needs its own number, not one overwriting the last.
    public sealed class DamageNumberHud
    {
        private sealed class FloatingNumber
        {
            public string Text;
            public bool IsHeal;
            public float TimeRemaining;
            public float HorizontalOffset;
        }

        private readonly float _worldOffset;
        private readonly float _riseDistance;
        private readonly float _duration;
        private readonly int _fontSize;
        private readonly float _horizontalScatter;

        private readonly List<FloatingNumber> _numbers = new();
        private GUIStyle _damageStyle;
        private GUIStyle _healStyle;

        public DamageNumberHud(float worldOffset, float riseDistance, float duration, int fontSize, float horizontalScatter)
        {
            _worldOffset = worldOffset;
            _riseDistance = riseDistance;
            _duration = duration;
            _fontSize = fontSize;
            _horizontalScatter = horizontalScatter;
        }

        private GUIStyle DamageStyle => GuiDrawUtils.GetOrCreate(ref _damageStyle, () => BuildStyle(Color.red));

        private GUIStyle HealStyle => GuiDrawUtils.GetOrCreate(ref _healStyle, () => BuildStyle(new Color(0.35f, 0.9f, 0.35f)));

        private GUIStyle BuildStyle(Color color)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = _fontSize
            };
            style.normal.textColor = color;
            return style;
        }

        public void ShowDamage(int amount) => Show($"-{amount}", isHeal: false);

        public void ShowHeal(int amount) => Show($"+{amount}", isHeal: true);

        private void Show(string text, bool isHeal)
        {
            _numbers.Add(new FloatingNumber
            {
                Text = text,
                IsHeal = isHeal,
                TimeRemaining = _duration,
                HorizontalOffset = Random.Range(-_horizontalScatter, _horizontalScatter)
            });
        }

        public void Tick(float deltaTime)
        {
            for (var i = _numbers.Count - 1; i >= 0; i--)
            {
                _numbers[i].TimeRemaining -= deltaTime;
                if (_numbers[i].TimeRemaining <= 0f)
                {
                    _numbers.RemoveAt(i);
                }
            }
        }

        public void Draw(Transform anchor)
        {
            if (_numbers.Count == 0 || anchor == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            // screenPoint is already a real screen-pixel position from WorldToScreenPoint - draw it
            // outside the reference-resolution scale so it isn't shifted off the player.
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            var previousColor = GUI.color;

            const float width = 120f;
            const float height = 28f;

            foreach (var number in _numbers)
            {
                var progress = 1f - Mathf.Clamp01(number.TimeRemaining / _duration);
                var worldPosition = anchor.position + Vector3.up * (_worldOffset + progress * _riseDistance);
                var screenPoint = camera.WorldToScreenPoint(worldPosition);
                if (screenPoint.z <= 0f)
                {
                    continue;
                }

                var rect = new Rect(
                    screenPoint.x - width / 2f + number.HorizontalOffset,
                    Screen.height - screenPoint.y - height / 2f,
                    width,
                    height);

                GUI.color = new Color(1f, 1f, 1f, 1f - progress);
                GuiDrawUtils.DrawLabelWithShadow(rect, number.Text, number.IsHeal ? HealStyle : DamageStyle);
            }

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }
    }
}
