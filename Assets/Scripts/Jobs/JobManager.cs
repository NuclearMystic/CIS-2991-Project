using System;
using System.Collections.Generic;

namespace CIS2991Project.Jobs
{
    // Plain static state (no scene object needed), same pattern as AudioManager - job progress and
    // completion naturally survive every scene load for the life of the session without any
    // DontDestroyOnLoad wiring, which is what lets a job accepted in one scene (e.g. the Settlement)
    // keep being tracked once the player moves to a different scene (e.g. the Overworld).
    public static class JobManager
    {
        public static event Action<JobDefinition> JobCompleted;

        private static readonly Dictionary<JobDefinition, int> ActiveProgress = new();
        private static readonly HashSet<JobDefinition> CompletedJobs = new();

        public static void StartJob(JobDefinition job)
        {
            if (job == null || IsJobActive(job) || IsJobCompleted(job))
                return;

            ActiveProgress[job] = 0;
        }

        public static bool IsJobActive(JobDefinition job) => job != null && ActiveProgress.ContainsKey(job);

        public static bool IsJobCompleted(JobDefinition job) => job != null && CompletedJobs.Contains(job);

        public static int GetProgress(JobDefinition job) => ActiveProgress.TryGetValue(job, out var progress) ? progress : 0;

        // For UI (job journal, HUD tracker) to list every job currently in that state.
        public static IReadOnlyCollection<JobDefinition> ActiveJobs => ActiveProgress.Keys;

        public static IReadOnlyCollection<JobDefinition> FinishedJobs => CompletedJobs;

        // Called from Enemy.Die() with the dying enemy's EnemyDefinition.displayName. Doesn't need
        // to know which enemy died beyond that tag, keeping Enemy.cs from knowing anything about jobs.
        public static void ReportKill(string killTag)
        {
            if (string.IsNullOrEmpty(killTag) || ActiveProgress.Count == 0)
                return;

            List<JobDefinition> completedThisCall = null;

            // Snapshot the keys first - updating ActiveProgress[job] below would otherwise invalidate
            // an in-progress enumeration of ActiveProgress.Keys directly.
            var activeJobs = new List<JobDefinition>(ActiveProgress.Keys);
            foreach (var job in activeJobs)
            {
                if (job.killTargetTag != killTag)
                    continue;

                var progress = ActiveProgress[job] + 1;
                ActiveProgress[job] = progress;

                if (progress >= job.killTargetCount)
                {
                    completedThisCall ??= new List<JobDefinition>();
                    completedThisCall.Add(job);
                }
            }

            if (completedThisCall == null)
                return;

            foreach (var job in completedThisCall)
            {
                ActiveProgress.Remove(job);
                CompletedJobs.Add(job);
                JobCompleted?.Invoke(job);
            }
        }

        // This static state is deliberately never cleared by a scene load (that's the whole point -
        // see the class comment), but that means it also survives death, so a fresh playthrough would
        // otherwise start with every job from the last one already "completed" (e.g. JobSpawnGate
        // permanently hiding an enemy group). Called when leaving gameplay entirely, from the same
        // "Return to Main Menu" handlers that reset PauseGate and deactivate the Player rig.
        public static void ResetAll()
        {
            ActiveProgress.Clear();
            CompletedJobs.Clear();
        }

        // Replaces all job state in one shot - used by SaveSystem when loading a save.
        public static void LoadState(IEnumerable<(JobDefinition job, int progress)> activeJobs, IEnumerable<JobDefinition> completedJobs)
        {
            ActiveProgress.Clear();
            CompletedJobs.Clear();

            if (activeJobs != null)
            {
                foreach (var (job, progress) in activeJobs)
                {
                    if (job != null)
                    {
                        ActiveProgress[job] = progress > 0 ? progress : 0;
                    }
                }
            }

            if (completedJobs != null)
            {
                foreach (var job in completedJobs)
                {
                    if (job != null)
                    {
                        CompletedJobs.Add(job);
                    }
                }
            }
        }
    }
}
