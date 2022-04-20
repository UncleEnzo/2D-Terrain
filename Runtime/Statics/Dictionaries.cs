using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public static class Dictionaries
    {
        public static ReadOnlyDictionary<string, int> SortingLayers = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>() {
            { "BelowGround", 0 },
            { "Ground", 1 },
            { "OverGround", 2 },
            { "Wall", 3 },
            { "Default", 4 },
        });

        public static ReadOnlyDictionary<string, int> SortingLayersNoWall = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(){
            { "BelowGround", 0 },
            { "Ground", 1 },
            { "OverGround", 2 },
            //{ "Wall", 3 },
            { "Default", 4 },
        });

        private static ReadOnlyDictionary<TileBase, TileData> dataFromTiles = null;
        public static ReadOnlyDictionary<TileBase, TileData> DataFromTiles
        {
            get
            {
                if (dataFromTiles == null)
                {
                    Dictionary<TileBase, TileData> tileDatas = new Dictionary<TileBase, TileData>();
                    foreach (var tileData in Resources.LoadAll(Constants.TILE_TYPES_PATH))
                    {
                        TileData tileDataCast = tileData as TileData;
                        foreach (var tile in tileDataCast.tileset)
                        {
                            tileDatas.Add(tile, tileDataCast);
                        }
                    }
                    dataFromTiles = new ReadOnlyDictionary<TileBase, TileData>(tileDatas);
                }
                return dataFromTiles;
            }
        }
    }
}