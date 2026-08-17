using UnityEngine;

namespace CIS2991Project.Data
{
    // Resolves the stable string ids on Item/JobDefinition (see their id fields) back to the actual
    // asset reference. Needed because save files store ids, not the ScriptableObjects themselves -
    // Item/JobDefinition assets live under Assets/Scripts, not Resources, so there's no way to look
    // them up by id at runtime without a registry like this one. Lives at Resources/ItemDatabase.asset
    // so SaveSystem can load it with Resources.Load.
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Afterfall/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private global::Item[] items;
        [SerializeField] private CIS2991Project.Jobs.JobDefinition[] jobs;

        public global::Item GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null)
            {
                return null;
            }

            foreach (var item in items)
            {
                if (item != null && item.id == id)
                {
                    return item;
                }
            }

            return null;
        }

        public CIS2991Project.Jobs.JobDefinition GetJobById(string id)
        {
            if (string.IsNullOrEmpty(id) || jobs == null)
            {
                return null;
            }

            foreach (var job in jobs)
            {
                if (job != null && job.id == id)
                {
                    return job;
                }
            }

            return null;
        }
    }
}
