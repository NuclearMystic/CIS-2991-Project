using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Hunger/Thirst/Radiation meters (fill bars - same background+fill pattern as VitalsHud's reload
    // bar, the only "bar" precedent in this codebase) plus a full-screen haze overlay that intensifies
    // as SurvivalStats.Severity climbs. Radiation is a placeholder - a fixed display value, not wired
    // to any real system yet.
    public sealed class SurvivalHud
    {
        private readonly Texture2D _hungerBackgroundTexture;
        private readonly Texture2D _hungerFillTexture;
        private readonly Color _hungerFillColor;
        private readonly Texture2D _thirstBackgroundTexture;
        private readonly Texture2D _thirstFillTexture;
        private readonly Color _thirstFillColor;
        private readonly Texture2D _radiationBackgroundTexture;
        private readonly Texture2D _radiationFillTexture;
        private readonly Color _radiationFillColor;
        private readonly float _radiationPlaceholderValue;
        private readonly float _barWidth;
        private readonly float _barHeight;
        private readonly float _barGap;
        private readonly Color _hazeColor;
        private readonly float _maxHazeAlpha;
        private readonly Color _emptyPulseColor;
        private readonly float _emptyPulseSpeed;
        private readonly float _emptyPulseMaxAlpha;

        private Texture2D _solidTexture;

        public SurvivalHud(
            Texture2D hungerBackgroundTexture, Texture2D hungerFillTexture, Color hungerFillColor,
            Texture2D thirstBackgroundTexture, Texture2D thirstFillTexture, Color thirstFillColor,
            Texture2D radiationBackgroundTexture, Texture2D radiationFillTexture, Color radiationFillColor, float radiationPlaceholderValue,
            float barWidth, float barHeight, float barGap,
            Color hazeColor, float maxHazeAlpha,
            Color emptyPulseColor, float emptyPulseSpeed, float emptyPulseMaxAlpha)
        {
            _hungerBackgroundTexture = hungerBackgroundTexture;
            _hungerFillTexture = hungerFillTexture;
            _hungerFillColor = hungerFillColor;
            _thirstBackgroundTexture = thirstBackgroundTexture;
            _thirstFillTexture = thirstFillTexture;
            _thirstFillColor = thirstFillColor;
            _radiationBackgroundTexture = radiationBackgroundTexture;
            _radiationFillTexture = radiationFillTexture;
            _radiationFillColor = radiationFillColor;
            _radiationPlaceholderValue = radiationPlaceholderValue;
            _barWidth = barWidth;
            _barHeight = barHeight;
            _barGap = barGap;
            _hazeColor = hazeColor;
            _maxHazeAlpha = maxHazeAlpha;
            _emptyPulseColor = emptyPulseColor;
            _emptyPulseSpeed = emptyPulseSpeed;
            _emptyPulseMaxAlpha = emptyPulseMaxAlpha;
        }

        public float Draw(SurvivalStats survivalStats, float startX, float y)
        {
            if (survivalStats == null)
            {
                return y;
            }

            var rowY = y;

            DrawBar(new Rect(startX, rowY, _barWidth, _barHeight), survivalStats.Hunger, _hungerBackgroundTexture, _hungerFillTexture, _hungerFillColor, "Hunger", pulseWhenEmpty: true);
            rowY += _barHeight + _barGap;

            DrawBar(new Rect(startX, rowY, _barWidth, _barHeight), survivalStats.Thirst, _thirstBackgroundTexture, _thirstFillTexture, _thirstFillColor, "Thirst", pulseWhenEmpty: true);
            rowY += _barHeight + _barGap;

            DrawBar(new Rect(startX, rowY, _barWidth, _barHeight), _radiationPlaceholderValue, _radiationBackgroundTexture, _radiationFillTexture, _radiationFillColor, "Radiation", pulseWhenEmpty: false);
            rowY += _barHeight + _barGap;

            return rowY;
        }

        private void DrawBar(Rect rect, float value, Texture2D background, Texture2D fill, Color fillColor, string label, bool pulseWhenEmpty)
        {
            GuiDrawUtils.DrawSlot(rect, background);

            var fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value / SurvivalStats.MaxMeter), rect.height);
            DrawFill(fillRect, fill, fillColor);

            // The fill rect is zero-width at 0, so it can't carry a "you're starving/dehydrated"
            // warning by itself - flash the whole bar red instead, since that's the one state where
            // the bar would otherwise look inert.
            if (pulseWhenEmpty && value <= 0f)
            {
                DrawEmptyPulse(rect);
            }

            GUI.Label(rect, label, GuiDrawUtils.CenteredLabelStyle);
        }

        private void DrawEmptyPulse(Rect rect)
        {
            EnsureSolidTexture();

            var pulse = (Mathf.Sin(Time.time * _emptyPulseSpeed) + 1f) * 0.5f;
            var alpha = pulse * _emptyPulseMaxAlpha;

            var previousColor = GUI.color;
            GUI.color = new Color(_emptyPulseColor.r, _emptyPulseColor.g, _emptyPulseColor.b, alpha);
            GUI.DrawTexture(rect, _solidTexture);
            GUI.color = previousColor;
        }

        // Custom fill art (if assigned in the Inspector) always wins; otherwise tints a shared solid
        // texture with fillColor, so these bars have a sensible default look with zero art required.
        private void DrawFill(Rect rect, Texture2D fill, Color fillColor)
        {
            if (fill != null)
            {
                GuiDrawUtils.DrawSlot(rect, fill);
                return;
            }

            EnsureSolidTexture();

            var previousColor = GUI.color;
            GUI.color = fillColor;
            GUI.DrawTexture(rect, _solidTexture);
            GUI.color = previousColor;
        }

        public void DrawHazeOverlay(SurvivalStats survivalStats)
        {
            if (survivalStats == null)
            {
                return;
            }

            var alpha = survivalStats.Severity * _maxHazeAlpha;
            if (alpha <= 0f)
            {
                return;
            }

            EnsureSolidTexture();

            var previousColor = GUI.color;
            GUI.color = new Color(_hazeColor.r, _hazeColor.g, _hazeColor.b, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, GuiScale.ReferenceWidth, GuiScale.ReferenceHeight), _solidTexture);
            GUI.color = previousColor;
        }

        private void EnsureSolidTexture()
        {
            if (_solidTexture != null)
            {
                return;
            }

            _solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _solidTexture.SetPixel(0, 0, Color.white);
            _solidTexture.Apply();
        }
    }
}
