using Nevelson.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PitfallObject))]
    public abstract class MovementBase : TerrainBase
    {
        protected Rigidbody2D rigidBody;
        private TileData defaultTileProperties;
        private IPitfall iPitfall;
        private TileData previousTileData;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            iPitfall = GetComponent<IPitfall>();
            defaultTileProperties = Resources.Load(Constants.TILE_TYPES_PATH + Constants.DEFAULT_TILE) as TileData;
            previousTileData = defaultTileProperties;
        }

        protected void TraverseTile(Vector2 moveVelocity)
        {
            if (!TryGetTopMapNoWall(transform.Position2D(), LevelTerrain.Tilemaps, out Tilemap surfaceMap))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, Vector2.zero, iPitfall);
                return;
            }

            if (surfaceMap == null)
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, Vector2.zero, iPitfall);
                return;
            }

            Vector3Int gridPosition = surfaceMap.WorldToCell(transform.Position2D());
            Vector2 cellCenter = surfaceMap.GetCellCenterWorld(gridPosition);
            TileBase tile = surfaceMap.GetTile(gridPosition);
            if (!TryGetTileData(tile, out TileData tileProperties))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, cellCenter, iPitfall);
                return;
            }

            if (tileProperties.IsMovingPlatform)
            {
                tileProperties.movePlatformVelocity = surfaceMap.transform.GetComponentInParent<IMovingPlatform>().MoveVelocity;
            }

            previousTileData = tileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, cellCenter, iPitfall);
        }

        private bool TryGetTopMapNoWall(Vector2 worldPosition, List<Tilemap> tileMaps, out Tilemap surfaceMap)
        {
            surfaceMap = null;
            List<Tilemap> mapsAtWorldPosition = GetMapsAtPos(worldPosition, tileMaps);
            if (mapsAtWorldPosition.Count == 0)
            {
                return false;
            }

            if (!TryGetTopSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayersNoWall, out int surfaceLayer))
            {
                return false;
            }

            Tilemap[] topLayerTilemaps = GetTilemapsOfLayer(mapsAtWorldPosition, surfaceLayer);
            if (topLayerTilemaps.Length == 0)
            {
                return false;
            }
            else if (topLayerTilemaps.Length == 1)
            {
                surfaceMap = topLayerTilemaps[0];
                return true;
            }
            else
            {
                return GetLargestSortOrderTilemap(topLayerTilemaps, out surfaceMap);
            }
        }

        private Tilemap[] GetTilemapsOfLayer(List<Tilemap> mapsAtWorldPosition, int targetSortLayer)
        {
            List<Tilemap> mapsOfTargetLayer = new List<Tilemap>();
            foreach (var map in mapsAtWorldPosition)
            {
                string sortLayerName = map.GetComponent<TilemapRenderer>().sortingLayerName;
                if (Dictionaries.SortingLayers[sortLayerName] == targetSortLayer)
                {
                    mapsOfTargetLayer.Add(map);
                }
            }

            return mapsOfTargetLayer.ToArray();
        }


        private bool GetLargestSortOrderTilemap(Tilemap[] sameLayerTilemaps, out Tilemap largestSortOrderTileMap)
        {
            //Setting to weird number so that it takes first layer as the reference for duplicate checking
            int largestSortOrder = -10000000;
            Tilemap largestTilemap = null;
            foreach (var map in sameLayerTilemaps)
            {
                int newMapSortOrder = map.GetComponent<TilemapRenderer>().sortingOrder;
                if (largestSortOrder == newMapSortOrder)
                {
                    Debug.LogError($"Tilemap collision occurred. {gameObject.name} is standing on 2 or more tiles maps with same sorting layer AND sort order.  Probably due to overlapping rooms.  Make sure tile maps {largestTilemap.name} and {map.name} don't overlap or change the sort order of one of them.");
                    largestSortOrderTileMap = null;
                    return false;
                }
                if (newMapSortOrder > largestSortOrder)
                {
                    largestTilemap = map;
                    largestSortOrder = newMapSortOrder;
                }
            }

            largestSortOrderTileMap = largestTilemap;
            return true;
        }

        private bool TryGetTileData(TileBase tile, out TileData tileData)
        {
            if (tile == null)
            {
                tileData = null;
                return false;
            }

            if (!Dictionaries.DataFromTiles.TryGetValue(tile, out tileData))
            {
                return false;
            }

            return true;
        }
    }
}