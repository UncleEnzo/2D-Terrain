using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public abstract class TerrainBase : MonoBehaviour
    {
        protected SortingLayers sortingLayers;
        protected virtual void OnEnable()
        {
            SortingLayers layersSO = Resources.Load(Constants.SORTING_LAYERS_PATH) as SortingLayers;
            if (layersSO == null)
            {
                Debug.LogError($"Could not load layers scriptable object.  Create a SortingLayers ScriptableObject called SortingLayers in your Resources/ScriptableObjects/ folder");
            }
            sortingLayers = layersSO;
        }

        protected Vector2 GetTileCenter(Tilemap tilemap, Vector2 position)
        {
            Vector3Int gridPosition = tilemap.WorldToCell(position);
            return tilemap.GetCellCenterWorld(gridPosition);
        }

        protected TileBase GetTileBaseOnPoint(Tilemap tilemap, Vector2 position)
        {
            Vector3Int gridPosition = tilemap.WorldToCell(position);
            return tilemap.GetTile(gridPosition);
        }

        protected bool TryGetTopMapNoWall(Vector2 worldPosition, List<Tilemap> tileMaps, out Tilemap surfaceMap)
        {
            surfaceMap = null;
            List<Tilemap> mapsAtWorldPosition = GetMapsAtPos(worldPosition, tileMaps);
            if (mapsAtWorldPosition.Count == 0)
            {
                return false;
            }

            //NOTE: THIS NEEDS TO BE EXCLUDABLE CAUSE WE'RE PASSING IN A SPECIAL DICT HERE
            if (!TryGetTopSortLayerAtPos(mapsAtWorldPosition, sortingLayers, out int surfaceLayer, true))
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

        protected bool TryGetTopSortLayerAtPos(List<Tilemap> mapsAtWorldPosition, SortingLayers sortingLayers, out int sortingMapLayer, bool excludeTopLayers = false)
        {
            //Filters out tilemaps that are not in the specified sortingLayers dictionary
            for (int i = 0; i < mapsAtWorldPosition.Count; i++)
            {
                string sortingName = mapsAtWorldPosition[i].GetComponent<TilemapRenderer>().sortingLayerName;
                if (!sortingLayers.ContainsKey(sortingName, excludeTopLayers))
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

            string sortingLayer = mapsAtWorldPosition[0].GetComponent<TilemapRenderer>().sortingLayerName;
            int surfaceMapLayer = sortingLayers.GetValue(sortingLayer, excludeTopLayers);
            foreach (var map in mapsAtWorldPosition)
            {
                //Greatest sorting layer of maps so only movement properties from top layer are applied
                //i.e Ground tile over Pit tile, only applies ground movement
                string sortingLayerName = map.GetComponent<TilemapRenderer>().sortingLayerName;
                if (!sortingLayers.TryGetValue(sortingLayerName, out int newMapSortingLayer, excludeTopLayers))
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

        protected List<Tilemap> GetMapsAtPos(Vector2 worldPosition, List<Tilemap> tileMaps)
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

        private Tilemap[] GetTilemapsOfLayer(List<Tilemap> mapsAtWorldPosition, int targetSortLayer)
        {
            List<Tilemap> mapsOfTargetLayer = new List<Tilemap>();
            foreach (var map in mapsAtWorldPosition)
            {
                string sortLayerName = map.GetComponent<TilemapRenderer>().sortingLayerName;
                if (sortingLayers.GetValue(sortLayerName) == targetSortLayer)
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
    }
}