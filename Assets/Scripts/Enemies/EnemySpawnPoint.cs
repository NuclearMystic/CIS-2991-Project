using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Place one of these anywhere in a level to have an enemy instantiated there when the scene
    // loads. Assign the NPC prefab (with an Enemy component + EnemyDefinition already set up on it).
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private Enemy enemyPrefab;

        private void Start()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyPrefab.PatrolRadius);
        }
    }
}
