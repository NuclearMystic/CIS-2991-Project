using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Floats a "ITEM NAME xN" popup above the player for a moment when an item is picked up.
    public sealed class PickupPopupHud
    {
        private readonly float _worldOffset;
        private readonly float _riseDistance;
        private readonly float _duration;
        private readonly int _fontSize;

        private GUIStyle _style;
        private string _text;
        private float _timeRemaining;

        public PickupPopupHud(float worldOffset, float riseDistance, float duration, int fontSize)
        {
            _worldOffset = worldOffset;
            _riseDistance = riseDistance;
            _duration = duration;
            _fontSize = fontSize;
        }

        private GUIStyle Style => GuiDrawUtils.GetOrCreate(ref _style, () =>
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = _fontSize
            };
            style.normal.textColor = Color.green;
            return style;
        });

        public void Show(global::Item item, int amount)
        {
            var itemName = GuiDrawUtils.GetItemName(item).ToUpperInvariant();
            _text = $"{itemName} X{amount}";
            _timeRemaining = _duration;
        }

        public void Tick(float deltaTime)
        {
            if (_timeRemaining > 0f)
            {
                _timeRemaining -= deltaTime;
            }
        }

        public void Draw(PlayerInventory playerInventory)
        {
            if (_timeRemaining <= 0f || string.IsNullOrEmpty(_text) || playerInventory == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var progress = 1f - Mathf.Clamp01(_timeRemaining / _duration);
            var worldPosition = playerInventory.transform.position + Vector3.up * (_worldOffset + progress * _riseDistance);
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            const float width = 320f;
            const float height = 28f;
            var rect = new Rect(screenPoint.x - width / 2f, Screen.height - screenPoint.y - height / 2f, width, height);

            // screenPoint is already a real screen-pixel position from WorldToScreenPoint - draw it
            // outside the reference-resolution scale so it isn't shifted off the player.
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 1f - progress);

            GuiDrawUtils.DrawLabelWithShadow(rect, _text, Style);

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }
    }
}
