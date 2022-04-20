using Nevelson.Utils;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public class TerrainMonoSingleton : MonoSingleton<TerrainMonoSingleton>
    {
        public static List<Tilemap> LevelTiles = new List<Tilemap>();
    }
}
