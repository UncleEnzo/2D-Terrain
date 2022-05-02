using Nevelson.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Nevelson.Terrain.Enums;

namespace Nevelson.Terrain
{
    public class PitfallObject : TerrainBase, IPitfall
    {
        //TODO EXTERNAL POSITION SETTING IS UGLY
        //Used In Disc.cs in frisbros Before a throw to set specific pitfall pos and respawn locations list
        //make these Getters and setters maybe? 

        //FIGURE OUT WHICH VALUES TO HIDE WITH ODIN IF UNUSED
        [SerializeField] private RespawnMode respawnMode = RespawnMode.MANUAL;
        public Vector2 pitfallPosition = Vector2.zero;
        [Tooltip("The transform locations where objects respawn. This value can be changed at runtime.")]
        public Transform[] customRespawnLocations;

        [Tooltip("How much time object has to exit pit before fall is triggered.")]
        [SerializeField] private float pitfallRecoveryTime = 0f;
        [Tooltip("Controls how quickly the falling animations player")]
        [SerializeField] private float pitfallAnimSpeed = .015f;
        [SerializeField] private bool isDestroyedOnPitfall = true;
        [Tooltip("Example unsafe layers > Wall, Object")]
        [SerializeField] private LayerMask unsafeRespawnLayers;
        [Tooltip("Minimum amount of space needed at respawn point for object to respawn")]
        [SerializeField] private float minRespawnSpace = .5f;


        [Tooltip("How many safe tiles do we store from the past, less tiles is more performant, but more change of returning error")]
        [SerializeField] [Range(2, 10)] private int cachedSafeTilesCount = 3;
        [SerializeField] public TilePosition[] safeTiles;


        private bool delayCompleted = false;
        private bool isFalling = false;
        private bool isFirstPitContact = true;
        private IPitfallCondition[] pitfallChecks;
        private IPitfallStates[] pitfallObjs;
        private Action BeforePitfall;
        private Action DuringPitfall;
        private Action AfterPitfall;
        private Coroutine delayPitfallCo = null;

        private void OnEnable()
        {
            safeTiles = new TilePosition[cachedSafeTilesCount];
            pitfallChecks = GetComponents<IPitfallCondition>();
            pitfallObjs = GetComponents<IPitfallStates>();
            if (pitfallObjs == null || pitfallObjs.Length < 1)
            {
                Debug.LogError($"{gameObject.name} needs to implement IPitfallStates interface on top level for pitfall to work.");
            }
            AfterPitfall += ResetFallingCheck;
            foreach (var pitfallObj in pitfallObjs)
            {
                BeforePitfall += pitfallObj.PF_Before;
                DuringPitfall += pitfallObj.PF_During;
                AfterPitfall += pitfallObj.PF_After;
            }
        }

        private void OnDisable()
        {
            AfterPitfall -= ResetFallingCheck;
            foreach (var pitfallObj in pitfallObjs)
            {
                BeforePitfall -= pitfallObj.PF_Before;
                AfterPitfall -= pitfallObj.PF_After;
            }
        }

        private void FixedUpdate()
        {
            if (respawnMode == RespawnMode.AUTOMATIC)
            {
                CollectLatestSafeTiles();
            }
        }

        private void CollectLatestSafeTiles()
        {
            if (isFalling)
            {
                return;
            }

            if (!IsPositionRespawnFriendly(transform.Position2D(), LevelTerrain.Tilemaps))
            {
                return;
            }

            if (!TryGetTopMapNoWall(transform.Position2D(), LevelTerrain.Tilemaps, out Tilemap topTileMap))
            {
                return;
            }

            bool isDone = false;
            for (int i = 0; i < safeTiles.Length; i++)
            {
                if (safeTiles[i] == null)
                {
                    safeTiles[i] = new TilePosition(topTileMap, transform.Position2D());
                    isDone = true;
                    break;
                }

                Vector2 currentTilePos = GetTileCenter(topTileMap, transform.Position2D());
                if (currentTilePos == safeTiles[i].World2D)
                {
                    return;
                }
            }

            //Shifts array and places latest tile in last position
            if (!isDone)
            {
                Array.Copy(safeTiles, 1, safeTiles, 0, safeTiles.Length - 1);
                safeTiles[safeTiles.Length - 1] = new TilePosition(topTileMap, transform.Position2D());
            }
        }

        public void OnFixedUpdate_TriggerPitfall(bool triggerImmediate = false)
        {
            foreach (var pitfallCheck in pitfallChecks)
            {
                if (!pitfallCheck.PF_Check())
                {
                    return;
                }
            }

            if (!triggerImmediate)
            {
                if (delayPitfallCo == null && !delayCompleted && !isFalling)
                {
                    delayPitfallCo = StartCoroutine(DelayPitfallCo());
                }

                if (!delayCompleted || isFalling)
                {
                    return;
                }
            }

            if (isFirstPitContact)
            {
                isFirstPitContact = false;
            }

            if (isFalling)
            {
                return;
            }
            else
            {
                SetIsFalling();
            }

            BeforePitfall();

            switch (respawnMode)
            {
                case RespawnMode.AUTOMATIC:
                    StartCoroutine(AutomaticFallingCo());
                    break;
                case RespawnMode.MANUAL:
                    StartCoroutine(ManualFallingCo(customRespawnLocations, LevelTerrain.Tilemaps));
                    break;
                default:
                    Debug.LogError("Specified Respawn Mode Doesn't Exist.");
                    break;
            }
            isFirstPitContact = true;
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }

        private void SetIsFalling()
        {
            isFalling = true;
        }

        private void ResetFallingCheck()
        {
            isFalling = false;
        }

        private IEnumerator DelayPitfallCo()
        {
            yield return new WaitForSeconds(pitfallRecoveryTime);
            delayCompleted = true;
            yield return new WaitForFixedUpdate();
            delayCompleted = false;
            delayPitfallCo = null;
        }

        private IEnumerator AutomaticFallingCo()
        {
            Vector2 scaleReduction = new Vector2(pitfallAnimSpeed, pitfallAnimSpeed);
            while (transform.LocalScale2D().x > 0)
            {
                transform.LocalScale2D(transform.LocalScale2D() - scaleReduction);
                yield return new WaitForSeconds(.01f);
            }
            yield return new WaitForSecondsRealtime(.1f);
            DuringPitfall();

            if (!isDestroyedOnPitfall)
            {
                yield return new WaitForSecondsRealtime(.6f);

                bool isTransformSet = false;
                for (int i = safeTiles.Length - 1; i >= 0; i--)
                {
                    if (safeTiles[i] == null)
                    {
                        continue;
                    }

                    if (!IsPosFree(safeTiles[i].World2D))
                    {
                        continue;
                    }

                    transform.Position2D(safeTiles[i].World2D);
                    isTransformSet = true;
                    break;
                }

                if (!isTransformSet)
                {
                    Debug.LogError("Could not find safe place to respawn.  Cached safe tiles must have objects over them.  Recommend increasing cache safe tile count or decreasing space required for object to respawn");
                    if (customRespawnLocations.Length != 0)
                    {
                        transform.Position2D(customRespawnLocations[0].Position2D());
                    }
                    else
                    {
                        transform.Position2D(Vector2.zero);
                    }
                }

                AfterPitfall();
                transform.LocalScale2D(Vector2.one);
            }
            else
            {
                AfterPitfall();
                DestroySelf();
            }
        }

        //TODO:
        //   -Need a better way to set Custom Respawn Points array to this component
        //   -Need better way to set pitfallPosition
        private IEnumerator ManualFallingCo(Transform[] respawnLocationTransform, List<Tilemap> tilemaps)
        {
            Vector2 scaleReduction = new Vector2(pitfallAnimSpeed, pitfallAnimSpeed);
            while (transform.LocalScale2D().x > 0)
            {
                transform.LocalScale2D(transform.LocalScale2D() - scaleReduction);
                yield return new WaitForSeconds(.01f);
            }
            yield return new WaitForSecondsRealtime(.1f);

            DuringPitfall();

            if (!isDestroyedOnPitfall)
            {
                yield return new WaitForSecondsRealtime(.6f);

                //while loop catches edgecase of moving respawn points that slide under walls etc
                List<Transform> respawnFriendlyPoints = new List<Transform>();
                while (respawnFriendlyPoints.Count == 0)
                {
                    foreach (var respawnPos in respawnLocationTransform)
                    {
                        if (IsPositionRespawnFriendly(respawnPos.transform.Position2D(), tilemaps))
                        {
                            bool addPoint = IsPosFree(respawnPos.transform.Position2D());
                            if (addPoint)
                            {
                                respawnFriendlyPoints.Add(respawnPos);
                            }
                        }
                        else
                        {
                            Debug.Log("Filtering out unsafe respawn point " + respawnPos.name);
                        }
                    }

                    if (respawnFriendlyPoints.Count == 0)
                    {
                        yield return null;
                    }
                }

                Transform respawnLocation;
                if (pitfallPosition != Vector2.zero)
                {
                    respawnLocation = pitfallPosition.GetClosest(respawnFriendlyPoints);
                }
                else
                {
                    respawnLocation = transform.Position2D().GetClosest(respawnFriendlyPoints);
                }

                transform.Position2D(respawnLocation.Position2D());

                //This is when falling is set to false, needs to happen after object has been moved
                //Since this is a coroutine, calculating object location and moving it may take more than a frame
                AfterPitfall();
                transform.LocalScale2D(Vector2.one);
            }
            else
            {
                AfterPitfall();
                DestroySelf();
            }
        }

        private bool IsPosFree(Vector2 respawnPos)
        {
            //Double check there is enough space for the obj to respawn
            Collider2D[] colliders = Physics2D.OverlapCircleAll(respawnPos, minRespawnSpace);

            bool addPoint = true;
            foreach (var collider in colliders)
            {
                //we are allowing trigger colliders
                if (!collider.isTrigger)
                {
                    addPoint = false;
                    continue;
                }
            }

            return addPoint;
        }

        private bool IsPositionRespawnFriendly(Vector2 worldPosition, List<Tilemap> tileMaps)
        {
            List<Tilemap> mapsAtWorldPosition = GetMapsAtPos(worldPosition, tileMaps);
            if (mapsAtWorldPosition.Count == 0)
            {
                return false;
            }

            if (!TryGetTopSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayers, out int largestLayer))
            {
                return true;
            }

            //Checks if the map is respawn friendly
            foreach (var respawnSortLayer in Dictionaries.RespawnFriendlySortingLayers.Values)
            {
                if (respawnSortLayer == largestLayer)
                {
                    return true;
                }
            }
            return false;
        }
    }
}