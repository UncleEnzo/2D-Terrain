using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public class TilemapSubscriber : MonoBehaviour
    {
        private List<Tilemap> tilemaps;
        private void Awake()
        {
            tilemaps = transform.GetComponentsInChildren<Tilemap>().ToList();
            foreach (var tilemap in tilemaps)
            {
                TerrainMonoSingleton.LevelTiles.Add(tilemap);
            }
        }

        private void OnDestroy()
        {
            foreach (var tilemap in tilemaps)
            {
                TerrainMonoSingleton.LevelTiles.Remove(tilemap);
            }
        }
    }
}