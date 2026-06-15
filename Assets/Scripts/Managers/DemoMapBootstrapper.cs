using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Managers
{
    public class DemoMapBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindObjectOfType<DemoMapBootstrapper>() != null)
            {
                return;
            }

            var bootstrapperObject = new GameObject("DemoMapBootstrapper");
            DontDestroyOnLoad(bootstrapperObject);
            bootstrapperObject.AddComponent<DemoMapBootstrapper>().Initialize();
        }

        private void Initialize()
        {
            CreateMap();
            CreateNpc();
        }

        private void CreateMap()
        {
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = new Color(0.16f, 0.18f, 0.2f);
            }

            CreateGround();
            CreateWall("North Wall", new Vector2(0f, 5.75f), new Vector2(14f, 1f));
            CreateWall("South Wall", new Vector2(0f, -5.75f), new Vector2(14f, 1f));
            CreateWall("West Wall", new Vector2(-6.75f, 0f), new Vector2(1f, 12f));
            CreateWall("East Wall", new Vector2(6.75f, 0f), new Vector2(1f, 12f));
            CreateWall("Crate 1", new Vector2(-1.5f, 1.25f), new Vector2(1.4f, 1.4f));
            CreateWall("Crate 2", new Vector2(2.25f, -0.75f), new Vector2(1.8f, 1.2f));
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = Vector3.zero;

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.24f, 0.25f, 0.22f));
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(13f, 11f);
        }

        private void CreateWall(string objectName, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(objectName);
            wall.transform.position = position;

            var spriteRenderer = wall.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.35f, 0.32f, 0.28f));
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = size;

            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.isTrigger = false;
        }

        private void CreateNpc()
        {
            var npc = new GameObject("Placeholder NPC");
            npc.transform.position = new Vector3(-3.25f, -1.75f, 0f);

            var spriteRenderer = npc.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.7f, 0.56f, 0.4f));
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(0.8f, 1.2f);

            var body = npc.AddComponent<BoxCollider2D>();
            body.size = new Vector2(0.8f, 1.2f);

            var dialogue = npc.AddComponent<NpcDialogue>();
            dialogue.Configure(
                "Survivor",
                "You made it to camp. Keep your supplies close out there."
            );
        }

        private Sprite CreateSolidSprite(Color color)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color32[16 * 16];
            var pixelColor = (Color32)color;

            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = pixelColor;
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
