using System;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Dialogue
{
    // Walk into range, press E to talk. Advances through DialogueTree.nodes one at a time; a node
    // with hasChoice set shows Yes/No buttons instead of a Continue prompt. Rebuilds
    // DemoMapBootstrapper's DemoNpcDialogue prototype as a real, reusable, data-driven component -
    // same floating-above-the-head anchoring (proven to avoid overlapping the corner HUD panels),
    // now driven by a DialogueTree asset instead of one hardcoded line.
    [RequireComponent(typeof(Collider2D))]
    public sealed class NpcDialogue : MonoBehaviour
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        // Only one NPC's dialogue box on screen at a time - opening one closes any other that's
        // open, same pattern as ChestInventory.ActiveChest.
        public static NpcDialogue ActiveDialogue { get; private set; }

        // Fired every time a node is displayed (including node 0 on open). Lets a future quest-giver
        // component react to "the player reached this specific line" (e.g. the Yes branch of a job
        // offer) without this script knowing anything about quests.
        public event Action<int> NodeReached;

        private bool _playerInRange;
        private bool _isOpen;
        private int _currentNodeIndex;

        private DialogueNode CurrentNode => dialogueTree.nodes[_currentNodeIndex];

        // For code-built NPCs (no scene/prefab Inspector to drag references into) - e.g.
        // DemoMapBootstrapper. Inspector-authored NPCs can just set the serialized fields directly.
        public void Configure(string displayName, DialogueTree tree)
        {
            npcName = displayName;
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

        private void OnDisable()
        {
            if (ActiveDialogue == this)
                Close();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
                _playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null)
                return;

            _playerInRange = false;
            if (ActiveDialogue == this)
                Close();
        }

        private void Update()
        {
            if (!_playerInRange)
                return;

            if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            if (!Input.GetKeyDown(interactKey))
                return;

            if (!_isOpen)
                Open();
            else if (!CurrentNode.hasChoice)
                Advance(CurrentNode.nextNodeIndex);
        }

        private void Open()
        {
            if (dialogueTree == null || dialogueTree.nodes.Count == 0)
                return;

            if (ActiveDialogue != null && ActiveDialogue != this)
                ActiveDialogue.Close();

            ActiveDialogue = this;
            _isOpen = true;

            var startIndex = dialogueTree.randomBark
                ? UnityEngine.Random.Range(0, dialogueTree.nodes.Count)
                : 0;
            GoToNode(startIndex);
        }

        public void Close()
        {
            _isOpen = false;
            if (ActiveDialogue == this)
                ActiveDialogue = null;
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
            if (!_playerInRange || Camera.main == null)
                return;

            var screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPoint.z < 0f)
                return;

            var anchorX = screenPoint.x;
            var anchorY = Screen.height - screenPoint.y;
            const float headClearance = 60f;

            const float promptWidth = 280f;
            const float promptHeight = 40f;
            var promptRect = new Rect(
                Mathf.Clamp(anchorX - promptWidth / 2f, 0f, Screen.width - promptWidth),
                anchorY - headClearance - promptHeight,
                promptWidth,
                promptHeight);

            if (!_isOpen)
            {
                GUI.Box(promptRect, string.Empty);
                GUI.Label(new Rect(promptRect.x + 12f, promptRect.y + 10f, promptRect.width - 24f, 20f), $"Press E to talk to {npcName}");
                return;
            }

            var node = CurrentNode;
            const float dialogueWidth = 380f;
            const float dialogueHeight = 150f;
            var dialogueRect = new Rect(
                Mathf.Clamp(anchorX - dialogueWidth / 2f, 0f, Screen.width - dialogueWidth),
                anchorY - headClearance - dialogueHeight,
                dialogueWidth,
                dialogueHeight);

            GUI.Box(dialogueRect, npcName);
            GUI.Label(new Rect(dialogueRect.x + 12f, dialogueRect.y + 28f, dialogueRect.width - 24f, 80f), node.text);

            if (node.hasChoice)
            {
                var buttonY = dialogueRect.y + dialogueRect.height - 36f;
                if (GUI.Button(new Rect(dialogueRect.x + 12f, buttonY, 80f, 26f), "Yes"))
                    Advance(node.yesNextNodeIndex);

                if (GUI.Button(new Rect(dialogueRect.x + dialogueRect.width - 92f, buttonY, 80f, 26f), "No"))
                    Advance(node.noNextNodeIndex);
            }
            else
            {
                GUI.Label(new Rect(dialogueRect.x + 12f, dialogueRect.y + dialogueRect.height - 26f, dialogueRect.width - 24f, 20f), "Press E to continue");
            }
        }
    }
}
