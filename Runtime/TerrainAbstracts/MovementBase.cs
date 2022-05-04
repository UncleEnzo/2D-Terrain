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
        private IPitfall iPitfall;
        private TileData defaultTileProperties;
        private TileData previousTileData;
        private Tilemap surfaceMap;
        private bool interactOccurred = false;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            iPitfall = GetComponent<IPitfall>();
            iInteractTiles = GetComponent<IInteractTiles>();
            defaultTileProperties = Resources.Load(Constants.TILE_TYPES_PATH + Constants.DEFAULT_TILE_PATH) as TileData;
            previousTileData = defaultTileProperties;
        }

        protected virtual void Update()
        {


            //This will be passing in the Interface
            //need to prevent it from occuring multiple times in a frame
            interactOccurred = ValidateTileInteraction();
        }

        private bool ValidateTileInteraction()
        {
            if (iInteractTiles == null)
            {
                return false;
            }

            if (surfaceMap == null)
            {
                return false;
            }

            TileBase tile = GetTileBaseOnPoint(surfaceMap, transform.Position2D());

            if (tile == null || !Tiles.DataFromTiles.TryGetValue(tile, out TileData tileProperties))
            {
                return defaultTileProperties.ApplyTileInteraction(iInteractTiles);
            }

            return tileProperties.ApplyTileInteraction(iInteractTiles);
        }

        protected void TraverseTile(Vector2 moveVelocity)
        {
            if (!TryGetTopMapNoWall(transform.Position2D(), LevelTerrain.Tilemaps, out surfaceMap))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, Vector2.zero, iPitfall);
                return;
            }

            if (surfaceMap == null)
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, Vector2.zero, iPitfall);
                return;
            }

            TileBase tile = GetTileBaseOnPoint(surfaceMap, transform.Position2D());
            Vector2 cellCenter = GetTileCenter(surfaceMap, transform.Position2D());

            if (!TryGetTileData(tile, out TileData tileProperties))
            {
                previousTileData = defaultTileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, cellCenter, iPitfall);
                return;
            }

            if (tileProperties.IsMovingPlatform)
            {
                tileProperties.MovePlatformVelocity = surfaceMap.transform.GetComponentInParent<IMovingPlatform>().MoveVelocity;
            }

            previousTileData = tileProperties.ApplyTileProperties(rigidBody, moveVelocity, previousTileData, cellCenter, iPitfall);
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