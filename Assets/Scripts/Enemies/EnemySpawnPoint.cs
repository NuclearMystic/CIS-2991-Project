using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Place one of these anywhere in a level to have an enemy instantiated there when the scene
    // loads. Assign the NPC prefab (with an Enemy component + EnemyDefinition already set up on it).
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private Enemy enemyPrefab;
        [Tooltip("How many enemies this point creates when the scene loads.")]
        [SerializeField, Min(1)] private int spawnCount = 1;
        [Tooltip("Additional enemies are spread randomly within this radius. The first enemy always spawns at the marker.")]
        [SerializeField, Min(0f)] private float groupRadius = 4f;

        private void Start()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            for (var spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
            {
                var offset = spawnIndex == 0 ? Vector2.zero : Random.insideUnitCircle * groupRadius;
                Instantiate(enemyPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(enemyPrefab.PatrolRadius, groupRadius));
        }
    }
}
