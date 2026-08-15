using UnityEngine;

namespace CIS2991Project.Levels
{
    // Gives a level-transition marker (e.g. a cave entrance) a visible placeholder graphic until
    // real art is assigned - same runtime-generated-solid-sprite pattern used by ChestInventory.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlaceholderMarkerSprite : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color fallbackColor = new Color(0.3f, 0.25f, 0.2f);

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite != null ? sprite : Fallback(fallbackColor);
        }

        private static Sprite Fallback(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
    }
}
