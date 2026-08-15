using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator, with one change: the source project's
    // PlayerRoom instantiated a player prefab into the room. CIS-2991-Project's player is a
    // persistent DontDestroyOnLoad object repositioned by GameBootstrapper/CaveDungeonBootstrapper,
    // not something a level spawns - so instead this places a visible entrance/exit marker at
    // roomCenter (the tile this room actually grew from, i.e. where the corridor connects - the
    // natural "cave mouth"), and picks a *different* open tile inside the room for the player to
    // land on via StaticPlacementHelper, so the player never spawns overlapping the entrance's
    // trigger. RoomContentGenerator removes this room from the pool before FightingPitRoom runs, so
    // it also stays enemy-free.
    public class PlayerRoom : RoomGenerator
    {
        [SerializeField] private GameObject entrancePrefab;
        [SerializeField] private PrefabPlacer prefabPlacer;

        // Where CaveDungeonBootstrapper should move the persistent player after this room is built.
        public Vector2 PlayerSpawnPosition { get; private set; }

        public override List<GameObject> ProcessRoom(
            Vector2Int roomCenter,
            HashSet<Vector2Int> roomFloor,
            HashSet<Vector2Int> roomFloorNoCorridors)
        {
            var placedObjects = new List<GameObject>();
            var entrancePosition = (Vector2)roomCenter + new Vector2(0.5f, 0.5f);

            if (entrancePrefab != null && prefabPlacer != null)
            {
                var entrance = prefabPlacer.CreateObject(entrancePrefab, entrancePosition);
                if (entrance != null)
                    placedObjects.Add(entrance);
            }

            // roomCenter is itself a corridor tile (that's how this room grew), so it's excluded
            // from roomFloorNoCorridors - the entrance's own tile can never come back as the spawn
            // pick below.
            var helper = new StaticPlacementHelper(roomFloor, roomFloorNoCorridors);
            var spawnSpot = helper.GetItemPlacementPosition(PlacementType.OpenSpace, 100, Vector2Int.one, false);
            PlayerSpawnPosition = spawnSpot.HasValue
                ? spawnSpot.Value + new Vector2(0.5f, 0.5f)
                : entrancePosition;

            return placedObjects;
        }
    }

    public abstract class PlacementData
    {
        [Min(0)]
        public int minQuantity = 0;
        [Min(0)]
        [Tooltip("Max is inclusive")]
        public int maxQuantity = 0;
        public int Quantity
            => UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
    }

    [Serializable]
    public class EnemyPlacementData : PlacementData
    {
        public GameObject enemyPrefab;
        public Vector2Int enemySize = Vector2Int.one;
    }

    // New - not part of the source project. Mirrors EnemyPlacementData's shape so
    // PrefabPlacer.PlaceChests can reuse the exact same placement logic as PlaceEnemies.
    [Serializable]
    public class ChestPlacementData : PlacementData
    {
        public GameObject chestPrefab;
        public Vector2Int chestSize = Vector2Int.one;
    }
}
