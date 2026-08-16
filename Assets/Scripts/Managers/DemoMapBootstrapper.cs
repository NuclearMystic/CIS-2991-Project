using CIS2991Project.Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIS2991Project.Managers
{
    public class DemoMapBootstrapper : MonoBehaviour
    {
        private const string PrototypeSceneName = "PrototypeLevel";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindAnyObjectByType<DemoMapBootstrapper>() != null)
            {
                return;
            }

            var bootstrapperObject = new GameObject("DemoMapBootstrapper");
            DontDestroyOnLoad(bootstrapperObject);
            bootstrapperObject.AddComponent<DemoMapBootstrapper>().Setup();
        }

        private bool _initialized;

        private void Setup()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;

            if (SceneManager.GetActiveScene().name == PrototypeSceneName)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (scene.name != PrototypeSceneName)
            {
                _initialized = false;
                return;
            }
            if (!_initialized)
                Initialize();
        }

        private void Initialize()
        {
            _initialized = true;
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
            //CreateRoad();
            CreateBoundary("North Wall", new Vector2(0f, 11.5f), new Vector2(28f, 1f));
            CreateBoundary("South Wall", new Vector2(0f, -11.5f), new Vector2(28f, 1f));
            CreateBoundary("West Wall", new Vector2(-13.5f, 0f), new Vector2(1f, 24f));
            CreateBoundary("East Wall", new Vector2(13.5f, 0f), new Vector2(1f, 24f));
            CreateBuilding("Shack A", new Vector2(-6.5f, 4f), new Vector2(4.4f, 3.6f));
            CreateBuilding("Shack B", new Vector2(6f, 2.5f), new Vector2(5.2f, 4f));
            CreateBuilding("Container", new Vector2(3f, -4.5f), new Vector2(3.6f, 2.4f));
            CreateDecor("Crate 1", new Vector2(-1.5f, -2.2f), new Vector2(1.6f, 1.6f), new Color(0.52f, 0.38f, 0.24f));
            CreateDecor("Crate 2", new Vector2(8.4f, -2.8f), new Vector2(1.8f, 1.8f), new Color(0.52f, 0.38f, 0.24f));
        }

        private void CreateGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = Vector3.zero;

            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(new Color(0.24f, 0.25f, 0.22f));
            spriteRenderer.sortingOrder = -1;
            ground.transform.localScale = new Vector3(26f, 22f, 1f);
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
            npc.tag = "NPC";
            npc.transform.position = new Vector3(-6.5f, -3.5f, 0f);

            var spriteRenderer = npc.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSolidSprite(Color.yellow);
            npc.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

            var body = npc.AddComponent<BoxCollider2D>();
            body.isTrigger = true;

            var dialogue = npc.AddComponent<NpcDialogue>();
            dialogue.Configure("Survivor", CreateDemoDialogueTree());
        }

        // Proves out branching (a line, then a Yes/No job offer, then a different response per
        // answer) without needing a persisted DialogueTree asset - matches this file's existing
        // fully-code-built convention (see CreateSolidSprite etc.) rather than introducing one.
        private static DialogueTree CreateDemoDialogueTree()
        {
            var tree = ScriptableObject.CreateInstance<DialogueTree>();

            tree.nodes.Add(new DialogueNode
            {
                text = "You made it to camp. Keep your supplies close out there.",
                nextNodeIndex = 1
            });
            tree.nodes.Add(new DialogueNode
            {
                text = "I could use an extra pair of hands around here. Interested in some work?",
                hasChoice = true,
                yesNextNodeIndex = 2,
                noNextNodeIndex = 3
            });
            tree.nodes.Add(new DialogueNode
            {
                text = "Good to hear. Come find me again once the job board's up and running.",
                nextNodeIndex = -1
            });
            tree.nodes.Add(new DialogueNode
            {
                text = "Fair enough. Door's always open if you change your mind.",
                nextNodeIndex = -1
            });

            return tree;
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
