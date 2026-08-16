using System.Collections.Generic;
using CIS2991Project.Dialogue;
using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.Jobs
{
    // Drop on the same NPC GameObject as NpcDialogue to turn it into a job giver: swaps which
    // DialogueTree plays based on job state (not started/in progress/completed), starts the job
    // when the player reaches the offer tree's Yes node, and hands out the reward the first time
    // the completion tree is shown.
    [RequireComponent(typeof(NpcDialogue))]
    public class JobGiver : MonoBehaviour
    {
        private enum JobDialogueState { Offer, InProgress, Complete }

        [SerializeField] private JobDefinition job;
        [SerializeField] private DialogueTree offerTree;
        [SerializeField] private DialogueTree inProgressTree;
        [SerializeField] private DialogueTree completeTree;

        [Tooltip("Node index in offerTree that means the player just said Yes.")]
        [SerializeField] private int acceptNodeIndex;

        [SerializeField] private List<global::ItemDrop> reward = new();

        private NpcDialogue _dialogue;
        private JobDialogueState _currentState;
        private bool _rewardGranted;

        private void Awake()
        {
            _dialogue = GetComponent<NpcDialogue>();
        }

        private void OnEnable()
        {
            _dialogue.BeforeOpen += HandleBeforeOpen;
            _dialogue.NodeReached += HandleNodeReached;
        }

        private void OnDisable()
        {
            _dialogue.BeforeOpen -= HandleBeforeOpen;
            _dialogue.NodeReached -= HandleNodeReached;
        }

        private void HandleBeforeOpen()
        {
            if (JobManager.IsJobCompleted(job))
            {
                _currentState = JobDialogueState.Complete;
                _dialogue.SetDialogueTree(completeTree);
            }
            else if (JobManager.IsJobActive(job))
            {
                _currentState = JobDialogueState.InProgress;
                _dialogue.SetDialogueTree(inProgressTree);
            }
            else
            {
                _currentState = JobDialogueState.Offer;
                _dialogue.SetDialogueTree(offerTree);
            }
        }

        private void HandleNodeReached(int nodeIndex)
        {
            if (_currentState == JobDialogueState.Offer && nodeIndex == acceptNodeIndex && !JobManager.IsJobActive(job))
            {
                JobManager.StartJob(job);
            }
            else if (_currentState == JobDialogueState.Complete && nodeIndex == 0 && !_rewardGranted)
            {
                GrantReward();
            }
        }

        private void GrantReward()
        {
            _rewardGranted = true;

            var inventory = Object.FindAnyObjectByType<PlayerInventory>();
            if (inventory == null || reward == null)
                return;

            foreach (var drop in reward)
            {
                if (drop.item != null)
                    inventory.TryAdd(drop.item, drop.RollAmount());
            }
        }
    }
}
