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

            if (!TryGetSortLayerNoWall(mapsAtWorldPosition, out int largestLayer))
            {
                return false;
            }

            surfaceMap = GetSurfaceMap(largestLayer, mapsAtWorldPosition);
            return true;
        }

        //Wrapper function that permits other sorting layer dictionary configurations
        private bool TryGetSortLayerNoWall(List<Tilemap> mapsAtWorldPosition, out int largestSortLayer)
        {
            return TryGetSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayersNoWall, out largestSortLayer);
        }

        private Tilemap GetSurfaceMap(int surfaceMapLayer, List<Tilemap> mapsAtWorldPosition)
        {
            List<Tilemap> mapsOfTargetLayer = new List<Tilemap>();
            int surfaceMapSortOrder = 0;
            surfaceMapSortOrder = GetLargestTopLayerSortOrder(surfaceMapLayer, mapsAtWorldPosition, mapsOfTargetLayer, surfaceMapSortOrder);

            foreach (var map in mapsOfTargetLayer)
            {
                int sortOrder = map.GetComponent<TilemapRenderer>().sortingOrder;
                if (sortOrder == surfaceMapSortOrder)
                {
                    return map;
                }
            }
            Debug.LogError("Could not get surface map");
            return null;
        }

        private int GetLargestTopLayerSortOrder(int surfaceMapLayer, List<Tilemap> mapsAtWorldPosition, List<Tilemap> mapsOfTargetLayer, int surfaceMapSortOrder)
        {
            foreach (var map in mapsAtWorldPosition)
            {
                //Collect all of the tilemaps you're standing on that are top sorting layer
                int newMapSortingLayer = Dictionaries.SortingLayers[map.GetComponent<TilemapRenderer>().sortingLayerName];
                if (newMapSortingLayer != surfaceMapLayer)
                {
                    continue;
                }
                mapsOfTargetLayer.Add(map);

                //Get largest sort order in top sorting layer
                int newMapSortOrder = map.GetComponent<TilemapRenderer>().sortingOrder;
                if (newMapSortOrder > surfaceMapSortOrder)
                {
                    surfaceMapSortOrder = newMapSortOrder;
                }
            }

            return surfaceMapSortOrder;
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