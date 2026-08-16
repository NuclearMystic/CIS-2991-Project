using System;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Small drawing/formatting helpers shared by every IMGUI panel in this project, so each one isn't
    // reimplementing the same texture-fallback box, UV-slice sprite draw, and lazy GUIStyle caching.
    public static class GuiDrawUtils
    {
        private static GUIStyle _centeredLabelStyle;

        // Plain GUI.Label has no background box, unlike GUI.Button, so it's safe to draw on top of a
        // slot's background texture without covering it back up.
        public static GUIStyle CenteredLabelStyle =>
            GetOrCreate(ref _centeredLabelStyle, () => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

        public static GUIStyle GetOrCreate(ref GUIStyle cache, Func<GUIStyle> factory)
        {
            return cache ??= factory();
        }

        public static void DrawSlot(Rect rect, Texture2D texture)
        {
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Box(rect, string.Empty);
            }
        }

        public static void DrawSprite(Rect rect, Sprite sprite)
        {
            var texture = sprite.texture;
            var textureRect = sprite.textureRect;
            var uv = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
            GUI.DrawTextureWithTexCoords(rect, texture, uv);
        }

        public static string GetItemName(global::Item item)
        {
            if (item == null)
            {
                return "Empty";
            }

            return string.IsNullOrWhiteSpace(item.displayName) ? item.name : item.displayName;
        }
    }
}
