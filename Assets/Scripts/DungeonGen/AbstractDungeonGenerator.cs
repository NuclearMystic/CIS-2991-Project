using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator (see plan for what was/wasn't ported).
    public abstract class AbstractDungeonGenerator : MonoBehaviour
    {
        [SerializeField]
        protected TilemapVisualizer tilemapVisualizer = null;
        [SerializeField]
        protected Vector2Int startPosition = Vector2Int.zero;

        public void GenerateDungeon()
        {
            tilemapVisualizer.Clear();
            RunProceduralGeneration();
        }

        protected abstract void RunProceduralGeneration();
    }
}
