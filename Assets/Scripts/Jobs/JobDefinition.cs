using UnityEngine;

namespace CIS2991Project.Jobs
{
    // One asset per job (kill-based jobs only, for now). killTargetTag is matched against
    // EnemyDefinition.displayName in JobManager.ReportKill, so it must match that field exactly
    // (e.g. "Zombie", "Raider").
    [CreateAssetMenu(fileName = "NewJob", menuName = "Afterfall/Job")]
    public class JobDefinition : ScriptableObject
    {
        public string jobName;
        public string killTargetTag;
        public int killTargetCount;
    }
}
