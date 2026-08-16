using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.Dialogue
{
    // One asset per NPC conversation. Paste each line into a new element of Nodes, chain them
    // with Next Node Index, and flip Has Choice on a line to end it in a Yes/No branch instead of
    // a Continue prompt. Unity's list Inspector already numbers elements ("Element 0", "Element 1",
    // ...), which is what the Next Node Index fields refer to.
    [CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Afterfall/Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        [Tooltip("For ambient background NPCs: talking to them shows one random line from Nodes each " +
                 "time instead of always starting at node 0. Each node should end the conversation " +
                 "(Next Node Index -1) rather than chaining, since there's no fixed sequence.")]
        public bool randomBark;

        public List<DialogueNode> nodes = new();
    }

    [Serializable]
    public struct DialogueNode
    {
        [TextArea(2, 6)]
        public string text;

        [Tooltip("If true, this line ends in a Yes/No choice instead of a Continue prompt.")]
        public bool hasChoice;

        [Tooltip("Next node index when there's no choice. -1 ends the conversation.")]
        public int nextNodeIndex;

        [Tooltip("Next node index after Yes. -1 ends the conversation.")]
        public int yesNextNodeIndex;

        [Tooltip("Next node index after No. -1 ends the conversation.")]
        public int noNextNodeIndex;
    }
}
