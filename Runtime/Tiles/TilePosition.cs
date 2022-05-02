using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public class TilePosition
    {
        private Tilemap tilemap;
        private Vector3Int gridPosition;

        public TilePosition(Tilemap tilemap, Vector2 position)
        {
            this.tilemap = tilemap;
            gridPosition = tilemap.WorldToCell(position);
        }

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