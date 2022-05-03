using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    /// <summary>
    /// Stores information about the tile at position including its future location in 
    /// local and world space. Useful for retrieving its coordinates after grid has moved.
    /// </summary>
    public class TilePosition
    {
        private Tilemap tilemap;
        private TileBase tileBase;
        private Vector3Int gridPosition;

        public TilePosition(Tilemap tilemap, Vector2 position)
        {
            this.tilemap = tilemap;
            gridPosition = tilemap.WorldToCell(position);
        }

        public TileBase TileBase { get => tileBase; }

        public Vector2 Local2D
        {
            get
            {
                return tilemap.GetCellCenterLocal(gridPosition);
            }
        }

        public Vector2 World2D
        {
            get
            {
                return tilemap.GetCellCenterWorld(gridPosition);
            }
        }
    }
}