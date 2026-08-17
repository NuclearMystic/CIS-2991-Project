using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CIS2991Project.DungeonGen
{
    // Ported from RogueAdjacent's procedural cave generator (see plan for what was/wasn't ported).
    public class TilemapVisualizer : MonoBehaviour
    {
        public Tilemap floorTilemap, wallTilemap;
        [SerializeField]
        private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull,
            wallInnerCornerDownLeft, wallInnerCornerDownRight, wallInnerCornerUpLeft, wallInnerCornerUpRight,
            wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

        // Built once from the fields above so PaintSingleBasicWall/PaintSingleCornerWall share one
        // "first matching set wins" lookup instead of each having its own near-identical if/else
        // chain. Order matters - preserves the original chains' exact priority.
        private (HashSet<int> set, TileBase tile)[] _basicWallRules;
        private (HashSet<int> set, TileBase tile)[] _cornerWallRules;

        private void Awake()
        {
            _basicWallRules = new (HashSet<int>, TileBase)[]
            {
                (WallTypesHelper.wallTop, wallTop),
                (WallTypesHelper.wallSideRight, wallSideRight),
                (WallTypesHelper.wallSideLeft, wallSiderLeft),
                (WallTypesHelper.wallBottm, wallBottom),
                (WallTypesHelper.wallFull, wallFull),
            };

            _cornerWallRules = new (HashSet<int>, TileBase)[]
            {
                (WallTypesHelper.wallInnerCornerDownLeft, wallInnerCornerDownLeft),
                (WallTypesHelper.wallInnerCornerDownRight, wallInnerCornerDownRight),
                (WallTypesHelper.wallInnerCornerUpRight, wallInnerCornerUpRight),
                (WallTypesHelper.wallInnerCornerUpLeft, wallInnerCornerUpLeft),
                (WallTypesHelper.wallDiagonalCornerDownLeft, wallDiagonalCornerDownLeft),
                (WallTypesHelper.wallDiagonalCornerDownRight, wallDiagonalCornerDownRight),
                (WallTypesHelper.wallDiagonalCornerUpRight, wallDiagonalCornerUpRight),
                (WallTypesHelper.wallDiagonalCornerUpLeft, wallDiagonalCornerUpLeft),
                (WallTypesHelper.wallFullEightDirections, wallFull),
                (WallTypesHelper.wallBottmEightDirections, wallBottom),
            };
        }

        public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
        {
            PaintTiles(floorPositions, floorTilemap, floorTile);
        }

        private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
        {
            foreach (var position in positions)
            {
                PaintSingleTile(tilemap, tile, position);
            }
        }

        internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
        {
            var tile = ResolveTile(_basicWallRules, Convert.ToInt32(binaryType, 2));
            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
        {
            var tile = ResolveTile(_cornerWallRules, Convert.ToInt32(binaryType, 2));
            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        private static TileBase ResolveTile((HashSet<int> set, TileBase tile)[] rules, int typeAsInt)
        {
            foreach (var rule in rules)
            {
                if (rule.set.Contains(typeAsInt))
                    return rule.tile;
            }

            return null;
        }

        private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)position);
            tilemap.SetTile(tilePosition, tile);
        }

        public void Clear()
        {
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();
        }
    }
}
