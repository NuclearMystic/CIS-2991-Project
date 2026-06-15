using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Managers
{
    public class DemoMapBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindAnyObjectByType<DemoMapBootstrapper>() != null)
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
            CreateRoad();
            CreateBoundary("North Wall", new Vector2(0f, 5.75f), new Vector2(14f, 1f));
            CreateBoundary("South Wall", new Vector2(0f, -5.75f), new Vector2(14f, 1f));
            CreateBoundary("West Wall", new Vector2(-6.75f, 0f), new Vector2(1f, 12f));
            CreateBoundary("East Wall", new Vector2(6.75f, 0f), new Vector2(1f, 12f));
            CreateBuilding("Shack A", new Vector2(-3.25f, 2f), new Vector2(2.2f, 1.8f));
            CreateBuilding("Shack B", new Vector2(3f, 1.25f), new Vector2(2.6f, 2f));
            CreateBuilding("Container", new Vector2(1.5f, -2.25f), new Vector2(1.8f, 1.2f));
            CreateDecor("Crate 1", new Vector2(-0.75f, -1.1f), new Vector2(0.8f, 0.8f), new Color(0.52f, 0.38f, 0.24f));
            CreateDecor("Crate 2", new Vector2(4.2f, -1.4f), new Vector2(0.9f, 0.9f), new Color(0.52f, 0.38f, 0.24f));
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = Vector3.zero;

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.24f, 0.25f, 0.22f));
            ground.transform.localScale = new Vector3(13f, 11f, 1f);
        }

        private void CreateRoad()
        {
            var road = new GameObject("Road");
            road.transform.position = new Vector3(0f, -0.15f, 0f);

            var spriteRenderer = road.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.28f, 0.27f, 0.25f));
            road.transform.localScale = new Vector3(10f, 1.4f, 1f);
        }

        private void CreateBoundary(string objectName, Vector2 position, Vector2 size)
        {
            CreateSolidBlock(objectName, position, size, new Color(0.38f, 0.33f, 0.29f));
        }

        private void CreateBuilding(string objectName, Vector2 position, Vector2 size)
        {
            CreateSolidBlock(objectName, position, size, new Color(0.46f, 0.42f, 0.36f));
        }

        private void CreateDecor(string objectName, Vector2 position, Vector2 size, Color color)
        {
            CreateSolidBlock(objectName, position, size, color);
        }

        private void CreateSolidBlock(string objectName, Vector2 position, Vector2 size, Color color)
        {
            var block = new GameObject(objectName);
            block.transform.position = position;

            var spriteRenderer = block.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(color);
            block.transform.localScale = new Vector3(size.x, size.y, 1f);

            var collider = block.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = false;
        }

        private void CreateNpc()
        {
            var npc = new GameObject("Placeholder NPC");
            npc.transform.position = new Vector3(-3.25f, -1.75f, 0f);

            var spriteRenderer = npc.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.7f, 0.56f, 0.4f));
            npc.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

            var body = npc.AddComponent<BoxCollider2D>();
            body.size = Vector2.one;
            body.offset = Vector2.zero;

            var dialogue = npc.AddComponent<DemoNpcDialogue>();
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

        private class DemoNpcDialogue : MonoBehaviour
        {
            private string npcName = "NPC";
            private string dialogueLine = "Hello.";
            private bool playerInRange;
            private bool dialogueOpen;

            public void Configure(string displayName, string line)
            {
                npcName = displayName;
                dialogueLine = line;
            }

            private void Reset()
            {
                GetComponent<Collider2D>().isTrigger = true;
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (other.GetComponentInParent<PlayerHealth>() != null)
                {
                    playerInRange = true;
                }
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                if (other.GetComponentInParent<PlayerHealth>() != null)
                {
                    playerInRange = false;
                    dialogueOpen = false;
                }
            }

            private void Update()
            {
                if (playerInRange && Input.GetKeyDown(KeyCode.E))
                {
                    dialogueOpen = !dialogueOpen;
                }

                if (dialogueOpen && Input.GetKeyDown(KeyCode.Escape))
                {
                    dialogueOpen = false;
                }
            }

            private void OnGUI()
            {
                if (!playerInRange)
                {
                    return;
                }

                GUI.Box(new Rect(16f, 150f, 280f, 70f), string.Empty);
                GUI.Label(new Rect(28f, 162f, 240f, 20f), $"Press E to talk to {npcName}");

                if (!dialogueOpen)
                {
                    return;
                }

                GUI.Box(new Rect(16f, 232f, 360f, 110f), npcName);
                GUI.Label(new Rect(28f, 260f, 320f, 60f), dialogueLine);
                GUI.Label(new Rect(28f, 316f, 260f, 20f), "Press Esc to close");
            }
        }
    }
}
