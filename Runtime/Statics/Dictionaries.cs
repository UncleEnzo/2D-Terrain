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
            { "Wall", 4 },
            { "Default", 5 },
        });

        public static ReadOnlyDictionary<string, int> SortingLayersNoWall = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(){
            { "BelowGround", 0 },
            { "Ground", 1 },
            { "Pitfall", 2 },
            { "OverGround", 3 },
            //{ "Wall", 4 },
            { "Default", 5 },
        });

        public static ReadOnlyDictionary<string, int> RespawnFriendlySortingLayers = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>() {
        { "BelowGround", 0 },
        { "Ground", 1 },
        { "Pitfall", 2 }, //Moving platforms TAKE THIS LAYER TOO FOR SOME REASON + Oring in layer 100 SHOULD PROBABLY JUST ADD A SEPARATE LAYER FOR THEM :D I THIKN IT'S A RESULT OF LAZINESS
        { "OverGround", 3 },
        { "JumpPad", 4 },
        //{ "ShortWall", 5 }, unfriendly
        { "Decorations", 6 },
        //{ "Effects", 7 }, unfriendly
        //{ "Hidden", 8 }, unfriendly
        //{ "Wall", 9 }, unfriendly
        //{ "OverWall", 10 }, unfriendly
        //{ "Default", 11 }, unfriendly
        //{ "Foreground", 12 }, unfriendly
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