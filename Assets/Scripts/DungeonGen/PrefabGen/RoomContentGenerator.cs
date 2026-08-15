using System.Linq;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator, minus the loot-table wiring
    // (GameManager.Instance/LootTable/InventoryItem - RogueAdjacent-only, not ported), the
    // optional GraphTest Dijkstra debug visualizer (not ported, not required for correctness), and
    // the Space-to-regenerate debug shortcut.
    public class RoomContentGenerator : MonoBehaviour
    {
        [SerializeField] private RoomGenerator playerRoom, defaultRoom;
        [SerializeField] private Transform itemParent;

        private readonly System.Collections.Generic.List<GameObject> spawnedObjects = new();

        public void GenerateRoomContent(DungeonData dungeonData)
        {
            CleanupSpawned();

            if (dungeonData.roomsDictionary == null || dungeonData.roomsDictionary.Count == 0)
            {
                Debug.LogWarning("No rooms to populate in dungeonData.");
                return;
            }

            SelectPlayerSpawnPoint(dungeonData);

            // Snapshot what's left so we don't iterate a collection we edit
            var remainingRooms = dungeonData.roomsDictionary.ToList();

            foreach (var room in remainingRooms)
            {
                var spawned = defaultRoom.ProcessRoom(
                    room.Key,
                    room.Value,
                    dungeonData.GetRoomFloorWithoutCorridors(room.Key)
                );
                spawnedObjects.AddRange(spawned);
            }

            ReparentSpawned(itemParent);
        }

        private void SelectPlayerSpawnPoint(DungeonData dungeonData)
        {
            int count = dungeonData.roomsDictionary.Count;
            if (count == 0)
            {
                Debug.LogError("roomsDictionary is empty - cannot pick a player spawn room.");
                return;
            }

            int randomRoomIndex = Random.Range(0, count);
            var kvp = dungeonData.roomsDictionary.ElementAt(randomRoomIndex);
            var playerSpawnPoint = kvp.Key;
            var roomTiles = kvp.Value;

            var placedPrefabs = playerRoom.ProcessRoom(
                playerSpawnPoint,
                roomTiles,
                dungeonData.GetRoomFloorWithoutCorridors(playerSpawnPoint)
            );

            spawnedObjects.AddRange(placedPrefabs);

            dungeonData.roomsDictionary.Remove(playerSpawnPoint);
        }

        private void CleanupSpawned()
        {
            // Prefer Destroy at runtime; DestroyImmediate is for editor-time ops.
            foreach (var go in spawnedObjects)
            {
                if (go != null) Destroy(go);
            }
            spawnedObjects.Clear();
        }

        private void ReparentSpawned(Transform parent)
        {
            if (parent == null) return;

            foreach (var go in spawnedObjects)
            {
                if (go != null)
                    go.transform.SetParent(parent, false);
            }
        }
    }
}
