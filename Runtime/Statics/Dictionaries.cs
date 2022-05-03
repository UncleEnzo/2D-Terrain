using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public static class Dictionaries
    {
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