using System;
using CIS2991Project.Core;
using CIS2991Project.UI;
using UnityEngine;

namespace CIS2991Project.Dialogue
{
    // Walk into range, press E to talk. Advances through DialogueTree.nodes one at a time; a node
    // with hasChoice set shows Yes/No buttons instead of a Continue prompt. Rebuilds
    // DemoMapBootstrapper's DemoNpcDialogue prototype as a real, reusable, data-driven component -
    // same floating-above-the-head anchoring (proven to avoid overlapping the corner HUD panels),
    // now driven by a DialogueTree asset instead of one hardcoded line.
    [RequireComponent(typeof(Collider2D))]
    public sealed class NpcDialogue : RangeInteractable<NpcDialogue>
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        // Only one NPC's dialogue box on screen at a time - opening one closes any other that's
        // open, same pattern as ChestInventory.ActiveChest.
        public static NpcDialogue ActiveDialogue => Active;

        // Fired every time a node is displayed (including node 0 on open). Lets a future quest-giver
        // component react to "the player reached this specific line" (e.g. the Yes branch of a job
        // offer) without this script knowing anything about quests.
        public event Action<int> NodeReached;

        // Fired right before Open() picks a start node. Lets a job-giver swap dialogueTree (via
        // Configure) based on current job state before the player sees anything.
        public event Action BeforeOpen;

        private int _currentNodeIndex;

        private DialogueNode CurrentNode => dialogueTree.nodes[_currentNodeIndex];

        // Boxes/text/buttons are drawn at real screen-pixel size (see the OnGUI comment on why
        // GuiScale isn't used here), so on a high-resolution display the original 1x sizing read as
        // tiny - these are 3x the original box/font/button dimensions.
        private GUIStyle _nameStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _promptStyle;
        private GUIStyle _buttonStyle;

        private GUIStyle NameStyle => GuiDrawUtils.GetOrCreate(ref _nameStyle, () => new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        });

        private GUIStyle BodyStyle => GuiDrawUtils.GetOrCreate(ref _bodyStyle, () => new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            wordWrap = true
        });

        private GUIStyle PromptStyle => GuiDrawUtils.GetOrCreate(ref _promptStyle, () => new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        });

        private GUIStyle ButtonStyle => GuiDrawUtils.GetOrCreate(ref _buttonStyle, () => new GUIStyle(GUI.skin.button)
        {
            fontSize = 30
        });

        // For code-built NPCs (no scene/prefab Inspector to drag references into) - e.g.
        // DemoMapBootstrapper. Inspector-authored NPCs can just set the serialized fields directly.
        public void Configure(string displayName, DialogueTree tree)
        {
            npcName = displayName;
            dialogueTree = tree;
        }

        // For a job-giver swapping which tree plays (Offer/InProgress/Complete) without touching
        // the NPC's already-authored display name.
        public void SetDialogueTree(DialogueTree tree)
        {
            dialogueTree = tree;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (!PlayerInRange)
                return;

            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            if (!Input.GetKeyDown(interactKey))
                return;

            if (!IsOpen)
                Open();
            else if (!CurrentNode.hasChoice)
                Advance(CurrentNode.nextNodeIndex);
        }

        private void Open()
        {
            BeforeOpen?.Invoke();

            if (dialogueTree == null || dialogueTree.nodes.Count == 0)
                return;

            BecomeActive();

            var startIndex = dialogueTree.randomBark
                ? UnityEngine.Random.Range(0, dialogueTree.nodes.Count)
                : 0;
            GoToNode(startIndex);
        }

        private void Advance(int nextIndex)
        {
            if (nextIndex < 0 || nextIndex >= dialogueTree.nodes.Count)
            {
                Close();
                return;
            }

            GoToNode(nextIndex);
        }

        private void GoToNode(int index)
        {
            _currentNodeIndex = index;
            NodeReached?.Invoke(index);
        }

        private void OnGUI()
        {
            // Deliberately not using GuiScale here: screenPoint comes from a real-screen-pixel
            // WorldToScreenPoint call, so scaling it the way PlayerHUD's fixed-corner panels are
            // scaled would double-apply and drift the box away from the NPC - same reasoning
            // PlayerHUD's reload bar/pickup popup already follow.
            if (!PlayerInRange || Camera.main == null)
                return;

            var screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPoint.z < 0f)
                return;

            var anchorX = screenPoint.x;
            var anchorY = Screen.height - screenPoint.y;
            const float headClearance = 60f;

            // 3x the original box/font/button sizes - see the field comment above.
            const float promptWidth = 840f;
            const float promptHeight = 120f;
            var promptRect = new Rect(
                Mathf.Clamp(anchorX - promptWidth / 2f, 0f, Screen.width - promptWidth),
                anchorY - headClearance - promptHeight,
                promptWidth,
                promptHeight);

            if (!IsOpen)
            {
                GUI.Box(promptRect, string.Empty);
                GUI.Label(new Rect(promptRect.x + 36f, promptRect.y + 30f, promptRect.width - 72f, 60f), $"Press E to talk to {npcName}", PromptStyle);
                return;
            }

            var node = CurrentNode;
            const float dialogueWidth = 1140f;
            const float dialogueHeight = 450f;
            var dialogueRect = new Rect(
                Mathf.Clamp(anchorX - dialogueWidth / 2f, 0f, Screen.width - dialogueWidth),
                anchorY - headClearance - dialogueHeight,
                dialogueWidth,
                dialogueHeight);

            GUI.Box(dialogueRect, string.Empty);
            GUI.Label(new Rect(dialogueRect.x, dialogueRect.y + 8f, dialogueRect.width, 44f), npcName, NameStyle);
            GUI.Label(new Rect(dialogueRect.x + 36f, dialogueRect.y + 84f, dialogueRect.width - 72f, 240f), node.text, BodyStyle);

            if (node.hasChoice)
            {
                var buttonY = dialogueRect.y + dialogueRect.height - 108f;
                if (GUI.Button(new Rect(dialogueRect.x + 36f, buttonY, 240f, 78f), "Yes", ButtonStyle))
                    Advance(node.yesNextNodeIndex);

                if (GUI.Button(new Rect(dialogueRect.x + dialogueRect.width - 276f, buttonY, 240f, 78f), "No", ButtonStyle))
                    Advance(node.noNextNodeIndex);
            }
            else
            {
                GUI.Label(new Rect(dialogueRect.x + 36f, dialogueRect.y + dialogueRect.height - 78f, dialogueRect.width - 72f, 60f), "Press E to continue", PromptStyle);
            }
        }
    }
}
