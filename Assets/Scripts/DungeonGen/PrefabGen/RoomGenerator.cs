using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator (see plan for what was/wasn't ported).
    public abstract class RoomGenerator : MonoBehaviour
    {
        public abstract List<GameObject> ProcessRoom(
            Vector2Int roomCenter,
            HashSet<Vector2Int> roomFloor,
            HashSet<Vector2Int> corridors);
    }
}
