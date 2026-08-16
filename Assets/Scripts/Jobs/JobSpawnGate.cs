using UnityEngine;

namespace CIS2991Project.Jobs
{
    // Drop on the parent GameObject of a group of EnemySpawnPoints. If jobDefinition is already
    // completed by the time this scene loads, deactivates the whole group before any child's
    // Start() runs (Unity always runs every Awake() in a scene before any Start()), so none of
    // them ever call Instantiate.
    public class JobSpawnGate : MonoBehaviour
    {
        [SerializeField] private JobDefinition jobDefinition;

        private void Awake()
        {
            if (jobDefinition != null && JobManager.IsJobCompleted(jobDefinition))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
