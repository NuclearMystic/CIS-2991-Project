using UnityEngine;

namespace CIS2991Project.Core
{
    // Runtime-generated placeholder sprites, used before real art is assigned. CreateCircleSprite was
    // duplicated identically between Enemy.cs and PlayerShoot.cs; CreateSolidSprite matches
    // ChestInventory's 1x1 fallback.
    public static class RuntimeSpriteUtils
    {
        public static Sprite CreateCircleSprite(Color color, int size = 16)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var center = (size - 1) / 2f;
            var radius = size / 2f - 0.5f;

            for (var i = 0; i < pixels.Length; i++)
            {
                float x = i % size;
                float y = i / size;
                pixels[i] = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) <= radius
                    ? (Color32)color
                    : new Color32(0, 0, 0, 0);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateSolidSprite(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
    }
}
