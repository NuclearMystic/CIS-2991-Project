using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator (see plan for what was/wasn't ported).
    public class DungeonData
    {
        public Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary;
        public HashSet<Vector2Int> floorPositions;
        public HashSet<Vector2Int> corridorPositions;

        public HashSet<Vector2Int> GetRoomFloorWithoutCorridors(Vector2Int dictionaryKey)
        {
            HashSet<Vector2Int> roomFloorNoCorridors = new HashSet<Vector2Int>(roomsDictionary[dictionaryKey]);
            roomFloorNoCorridors.ExceptWith(corridorPositions);
            return roomFloorNoCorridors;
        }
    }
}
