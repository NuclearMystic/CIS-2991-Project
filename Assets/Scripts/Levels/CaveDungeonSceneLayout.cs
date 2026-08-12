using UnityEngine;

namespace CIS2991Project.Levels
{
    // Placeholder cave interior that proves out the travel system - reskin/expand later.
    public sealed class CaveDungeonSceneLayout : MonoBehaviour
    {
        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            Block("Cave Floor", Vector2.zero, new Vector2(20f, 14f), new Color(0.16f, 0.15f, 0.18f), -20);
            Walls();
            Anchor("CaveDungeonSpawn", new Vector2(0f, 0f));
            Exit("Overworld Exit", new Vector2(0f, -5.5f), "Overworld");
        }

        private void Walls()
        {
            for (var x = -9.5f; x <= 9.5f; x += 1f)
            {
                Block("Cave Wall", new Vector2(x, 6.5f), new Vector2(1f, 1f), new Color(0.08f, 0.07f, 0.09f), -19);
                Block("Cave Wall", new Vector2(x, -6.5f), new Vector2(1f, 1f), new Color(0.08f, 0.07f, 0.09f), -19);
            }

            for (var y = -6f; y <= 6f; y += 1f)
            {
                Block("Cave Wall", new Vector2(9.5f, y), new Vector2(1f, 1f), new Color(0.08f, 0.07f, 0.09f), -19);
                Block("Cave Wall", new Vector2(-9.5f, y), new Vector2(1f, 1f), new Color(0.08f, 0.07f, 0.09f), -19);
            }
        }

        private static void Block(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = Solid(color);
            renderer.sortingOrder = order;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static void Anchor(string name, Vector2 position)
        {
            var anchor = new GameObject(name);
            anchor.transform.position = position;
        }

        private static void Exit(string name, Vector2 position, string destination)
        {
            var exit = new GameObject(name);
            exit.transform.position = position;
            exit.AddComponent<CircleCollider2D>().radius = 1.2f;
            exit.AddComponent<TeleportPoint>().Configure(destination);
        }

        private static Sprite Solid(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        }
    }
}
