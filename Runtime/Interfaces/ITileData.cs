using UnityEngine;

namespace Nevelson.Terrain
{
    public interface ITileData
    {
        TileData Get { get; }

        void ApplyTileSounds(ITileSound iTileSound);

        void ApplyTileInteraction(IInteractTiles iInteractTiles);

        TileData ApplyTileProperties(Rigidbody2D rigidbody, Vector2 moveVelocity, TileData previousTileData, Vector2 tilePos, IPitfall iPitfall);
    }
}