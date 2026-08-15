using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator, extended with chest placement
    // (chestPlacementData / PrefabPlacer.PlaceChests - see plan). itemData/PlaceAllItems from the
    // source project was dropped along with SceneStatic (see PrefabPlacer.cs).
    public class FightingPitRoom : RoomGenerator
    {
        [SerializeField]
        private PrefabPlacer prefabPlacer;

        public List<EnemyPlacementData> enemyPlacementData;
        public List<ChestPlacementData> chestPlacementData;

        public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
        {
            StaticPlacementHelper itemPlacementHelper =
                new StaticPlacementHelper(roomFloor, roomFloorNoCorridors);

            List<GameObject> placedObjects = new List<GameObject>();
            placedObjects.AddRange(prefabPlacer.PlaceEnemies(enemyPlacementData, itemPlacementHelper));
            placedObjects.AddRange(prefabPlacer.PlaceChests(chestPlacementData, itemPlacementHelper));

            return placedObjects;
        }
    }
}
