using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public abstract class TerrainBase : MonoBehaviour
    {
        protected bool TryGetSortLayerAtPos(List<Tilemap> mapsAtWorldPosition, ReadOnlyDictionary<string, int> sortingLayers, out int sortingMapLayer)
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
    }
}