using Nevelson.Utils;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PitfallObject))]
    public abstract class MovementBase : TerrainBase
    {
        protected Rigidbody2D rigidBody;
        private IInteractTiles iInteractTiles;
        private ITileSound iTileSound;
        private IPitfall iPitfall;
        private ITileData defaultTileProperties;
        private ITileData previousTileData;
        private Tilemap surfaceMap;
        private TileBase tileBase;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            iPitfall = GetComponentInChildren<IPitfall>();
            iInteractTiles = GetComponentInChildren<IInteractTiles>();
            iTileSound = GetComponentInChildren<ITileSound>();
            defaultTileProperties = Resources.Load(Constants.TILE_TYPES_PATH + Constants.DEFAULT_TILE_PATH) as ITileData;
            previousTileData = defaultTileProperties;
        }

        protected virtual void Update()
        {
            ValidateTileSound();
            ValidateTileInteraction();
        }

        private void ValidateTileSound()
        {
            if (iTileSound == null)
            {
                return;
            }

            if (surfaceMap == null)
            {
                iTileSound.PlayTileSound(null, false);
                return;
            }

            if (tileBase == null || !Tiles.DataFromTiles.TryGetValue(tileBase, out TileData tileProperties))
            {
                defaultTileProperties.ApplyTileSounds(iTileSound);
                return;
            }

            tileProperties.ApplyTileSounds(iTileSound);
        }

        private void ValidateTileInteraction()
        {
            if (iInteractTiles == null)
            {
                return;
            }

            if (surfaceMap == null)
            {
                iInteractTiles.InteractWithTile(null, false);
                return;
            }

            if (tileBase == null || !Tiles.DataFromTiles.TryGetValue(tileBase, out TileData tileProperties))
            {
                defaultTileProperties.ApplyTileInteraction(iInteractTiles);
                return;
            }

            tileProperties.ApplyTileInteraction(iInteractTiles);
        }

        protected void TraverseTile(Vector2 moveVelocity)
        {
            if (!TryGetTopMapNoWall(transform.Position2D(), LevelTerrain.Tilemaps, out surfaceMap))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData.Get, Vector2.zero, iPitfall);
                return;
            }

            if (surfaceMap == null)
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData.Get, Vector2.zero, iPitfall);
                return;
            }

            tileBase = GetTileBaseOnPoint(surfaceMap, transform.Position2D());
            Vector2 cellCenter = GetTileCenter(surfaceMap, transform.Position2D());

            if (!TryGetTileData(tileBase, out TileData tileProperties))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData.Get, cellCenter, iPitfall);
                return;
            }

            if (tileProperties.IsMovingPlatform)
            {
                tileProperties.MovePlatformVelocity = surfaceMap.transform.GetComponentInParent<IMovingPlatform>().MoveVelocity;
            }

            previousTileData = tileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData.Get, cellCenter, iPitfall);
        }

        private bool TryGetTileData(TileBase tile, out TileData tileData)
        {
            if (tile == null)
            {
                tileData = null;
                return false;
            }

            if (!Tiles.DataFromTiles.TryGetValue(tile, out tileData))
            {
                return false;
            }

            return true;
        }
    }
}