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
            { "Pitfall", 2 },
            { "OverGround", 3 },
            { "MovingPlatform", 4 },
            { "Wall", 5 },
            { "Default", 6 },
        });

        public static ReadOnlyDictionary<string, int> SortingLayersNoWall = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(){
            { "BelowGround", 0 },
            { "Ground", 1 },
            { "Pitfall", 2 },
            { "OverGround", 3 },
            { "MovingPlatform", 4 },
            //{ "Wall", 5 },
            { "Default", 6 },
        });

        public static ReadOnlyDictionary<string, int> RespawnFriendlySortingLayers = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>() {
            { "BelowGround", 0 },
            { "Ground", 1 },
            //{ "Pitfall", 2 }, unfriendly
            { "OverGround", 3 },
            { "MovingPlatform", 4 },
            //{ "Wall", 5 }, unfriendly
            { "Default", 6 },
            //{ "Effects", 8 }, unfriendly
            //{ "Hidden", 9 }, unfriendly
            //{ "Wall", 10 }, unfriendly
            //{ "OverWall", 11 }, unfriendly
            //{ "Default", 12 }, unfriendly
            //{ "Foreground", 13 }, unfriendly
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
                        foreach (var tile in tileDataCast.TileSet)
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