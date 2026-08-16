using System.Collections.Generic;
using UnityEngine;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator, minus PlaceAllItems/PlaceItem - those
    // depended on SceneStatic, which is entangled with RogueAdjacent-only singletons (GameManager,
    // SFXManager, LootTable) and wasn't ported (see plan). PlaceChests is new, mirroring
    // PlaceEnemies exactly for the same "just Instantiate a prefab" placement need.
    public class PrefabPlacer : MonoBehaviour
    {
        public List<GameObject> PlaceEnemies(List<EnemyPlacementData> enemyPlacementData,
                                             StaticPlacementHelper helper)
        {
            List<GameObject> placedObjects = new List<GameObject>();

            foreach (var placementData in enemyPlacementData)
            {
                for (int i = 0; i < placementData.Quantity; i++)
                {
                    Vector2? spot = helper.GetItemPlacementPosition(
                        PlacementType.OpenSpace,
                        100,
                        placementData.enemySize,
                        false
                    );

                    if (spot.HasValue)
                    {
                        placedObjects.Add(CreateObject(
                            placementData.enemyPrefab,
                            spot.Value + new Vector2(0.5f, 0.5f)));
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy placement failed for {placementData.enemyPrefab?.name} - no valid OpenSpace found.");
                    }
                }
            }
            return placedObjects;
        }

        public List<GameObject> PlaceChests(List<ChestPlacementData> chestPlacementData,
                                            StaticPlacementHelper helper)
        {
            List<GameObject> placedObjects = new List<GameObject>();

            foreach (var placementData in chestPlacementData)
            {
                for (int i = 0; i < placementData.Quantity; i++)
                {
                    Vector2? spot = helper.GetItemPlacementPosition(
                        PlacementType.NearWall,
                        100,
                        placementData.chestSize,
                        false
                    );

                    if (spot.HasValue)
                    {
                        placedObjects.Add(CreateObject(
                            placementData.chestPrefab,
                            spot.Value + new Vector2(0.5f, 0.5f)));
                    }
                    else
                    {
                        Debug.LogWarning($"Chest placement failed for {placementData.chestPrefab?.name} - no valid NearWall found.");
                    }
                }
            }
            return placedObjects;
        }

        public GameObject CreateObject(GameObject prefab, Vector3 pos)
        {
            if (prefab == null) return null;

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            return go;
        }
    }
}
