using Nevelson.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Nevelson.Terrain.Enums;


namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PitfallObject))]
    public abstract class MovementBase : MonoBehaviour
    {
        protected Rigidbody2D rigidBody;
        private MovementType previousMovementType = MovementType.TRANSFORM;
        private TileData defaultTileProperties;
        private IPitfall iPitfall;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            iPitfall = GetComponent<IPitfall>();
            defaultTileProperties = Resources.Load(Constants.TILE_TYPES_PATH + Constants.DEFAULT_TILE) as TileData;
        }

        protected void TraverseTile(Vector2 moveVelocity)
        {
            if (!TryGetTopMapNoWall(transform.Position2D(), LevelTerrain.Tilemaps, out Tilemap surfaceMap))
            {
                previousMovementType = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousMovementType, Vector2.zero, iPitfall);
                return;
            }

            if (surfaceMap == null)
            {
                previousMovementType = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousMovementType, Vector2.zero, iPitfall);
                return;
            }

            Vector3Int gridPosition = surfaceMap.WorldToCell(transform.Position2D());
            Vector2 cellCenter = surfaceMap.GetCellCenterWorld(gridPosition);
            TileBase tile = surfaceMap.GetTile(gridPosition);
            if (!TryGetTileData(tile, out TileData tileProperties))
            {
                previousMovementType = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousMovementType, cellCenter, iPitfall);
                return;
            }

            previousMovementType = tileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousMovementType, cellCenter, iPitfall);
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

        private List<Tilemap> GetMapsAtPos(Vector2 worldPosition, List<Tilemap> tileMaps)
        {
            List<Tilemap> mapsAtWorldPosition = new List<Tilemap>();
            foreach (var map in tileMaps)
            {
                //gets the tilebase of every single map 
                Vector3Int gridPos = map.WorldToCell(worldPosition);
                TileBase objOnTile = map.GetTile(gridPos);
                if (objOnTile == null)
                {
                    continue;
                }
                mapsAtWorldPosition.Add(map);
            }
            return mapsAtWorldPosition;
        }

        //Wrapper function that permits other sorting layer dictionary configurations
        private bool TryGetSortLayerNoWall(List<Tilemap> mapsAtWorldPosition, out int largestSortLayer)
        {
            return TryGetSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayersNoWall, out largestSortLayer);
        }

        private bool TryGetSortLayerAtPos(List<Tilemap> mapsAtWorldPosition, ReadOnlyDictionary<string, int> sortingLayers, out int sortingMapLayer)
        {
            //Filters out tilemaps that are not in the specified sortingLayers dictionary
            for (int i = 0; i < mapsAtWorldPosition.Count; i++)
            {
                string sortingName = mapsAtWorldPosition[i].GetComponent<TilemapRenderer>().sortingLayerName;
                if (!sortingLayers.ContainsKey(sortingName))
                {
                    mapsAtWorldPosition.Remove(mapsAtWorldPosition[i]);
                }
            }

            //If not standing on a tilemap at all (common in early projects without world built out)
            if (mapsAtWorldPosition.Count == 0)
            {
                sortingMapLayer = -1;
                return false;
            }

            int surfaceMapLayer = sortingLayers[mapsAtWorldPosition[0].GetComponent<TilemapRenderer>().sortingLayerName];
            foreach (var map in mapsAtWorldPosition)
            {
                //Greatest sorting layer of maps so only movement properties from top layer are applied
                //i.e Ground tile over Pit tile, only applies ground movement
                string sortingLayerName = map.GetComponent<TilemapRenderer>().sortingLayerName;
                if (!sortingLayers.TryGetValue(sortingLayerName, out int newMapSortingLayer))
                {
                    Debug.LogError("Sorting layer: " + sortingLayerName + " doesn't exist in sorting layers dictionary.");
                    sortingMapLayer = -1;
                    return false;
                }

                if (newMapSortingLayer > surfaceMapLayer)
                {
                    surfaceMapLayer = newMapSortingLayer;
                }
            }
            sortingMapLayer = surfaceMapLayer;
            return true;
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

        private static int GetLargestTopLayerSortOrder(int surfaceMapLayer, List<Tilemap> mapsAtWorldPosition, List<Tilemap> mapsOfTargetLayer, int surfaceMapSortOrder)
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