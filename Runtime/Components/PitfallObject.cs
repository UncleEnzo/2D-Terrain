using Nevelson.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nevelson.Terrain
{
    public class PitfallObject : MonoBehaviour, IPitfall
    {
        //TODO EXTERNAL POSITION SETTING IS UGLY
        //Used In Disc.cs in frisbros Before a throw to set specific pitfall pos and respawn locations list
        //make these Getters and setters maybe? 
        public Vector2 pitfallPosition = Vector2.zero;
        [Tooltip("The transform locations where objects respawn. This value can be changed at runtime.")]
        public Transform[] customRespawnLocations;

        [Tooltip("How much time object has to exit pit before fall is triggered.")]
        [SerializeField] private float pitfallRecoveryTime = 0f;
        [Tooltip("Distance obj autorespawns from pit")]
        [SerializeField] private float respawnDistFromPit = 2;
        [Tooltip("Controls how quickly the falling animations player")]
        [SerializeField] private float pitfallAnimSpeed = .015f;
        [SerializeField] private bool isDestroyedOnPitfall = true;

        private bool delayCompleted = false;
        private bool isFalling = false;
        private bool isFirstPitContact = true;
        private IPitfallCondition[] pitfallChecks;
        private IPitfallStates[] pitfallObjs;
        private Vector2 lastPosition = Vector2.zero;
        private Vector2 lastNonZeroMoveDirection = Vector2.zero;
        private Vector2 dynamicRespawnPos = Vector2.zero;
        private Action BeforePitfall;
        private Action DuringPitfall;
        private Action AfterPitfall;
        private Coroutine delayPitfallCo = null;

        private void OnEnable()
        {
            pitfallChecks = GetComponents<IPitfallCondition>();
            pitfallObjs = GetComponents<IPitfallStates>();
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

        private void LateUpdate()
        {
            DetermineRespawnLocation();
        }

        //Trigger immediate is useful for situations like when player is on a platform and falls in the direction the platform is moving. Prevents edgecase of platform catching up to him before trigger occurs
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
                dynamicRespawnPos = GetDynamicRespawnLocation();
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

            //dynamically chooses the respawn point if none is supplied
            if (customRespawnLocations.Length == 0)
            {
                //ToDo: Not implemented yet. I don't like how we CHOOSE the dynamic location.  need to make it a
                //position the player was at X seconds agos
                StartCoroutine(FallingCo());
            }
            //if there is a custom or multiple respawn points, runs checks to make sure the points are
            //not in a wall etc, then chooses the best one
            else
            {
                //pitfall ripple effect is what calls the audio
                StartCoroutine(FallingCo(customRespawnLocations, LevelTerrain.Tilemaps));
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

        private void DetermineRespawnLocation()
        {
            if (transform.Position2D() != lastPosition)
            {
                Vector2 moveDirection = (transform.Position2D() - lastPosition).normalized;
                if (moveDirection != Vector2.zero) lastNonZeroMoveDirection = moveDirection;
            }
            lastPosition = transform.Position2D();
        }

        private Vector2 GetDynamicRespawnLocation()
        {
            return transform.Position2D() +
                               -(lastNonZeroMoveDirection * respawnDistFromPit);
        }

        private IEnumerator DelayPitfallCo()
        {
            yield return new WaitForSeconds(pitfallRecoveryTime);
            delayCompleted = true;
            yield return new WaitForFixedUpdate();
            delayCompleted = false;
            delayPitfallCo = null;
        }

        //ToDO: NOT IMPLEMENTED YET, NEED TO FIGURE OUT A CLEANER DYNAMIC REPOSITION
        //LOOK TO BRACKEY'S VIDEO ON REWIND FEATURE AND MAYBE USE THAT POSITION TRACKING TO DETERMINE
        //THEN ADD X SECONDS AGO SO IT'S NOT RIGHT NEXT TO THE PIT
        private IEnumerator FallingCo()
        {
            Vector2 scaleReduction = new Vector2(pitfallAnimSpeed, pitfallAnimSpeed);
            while (gameObject.transform.LocalScale2D().x > 0)
            {
                gameObject.transform.LocalScale2D(gameObject.transform.LocalScale2D() - scaleReduction);
                yield return new WaitForSeconds(.01f);
            }
            yield return new WaitForSecondsRealtime(.1f);

            DuringPitfall();

            if (!isDestroyedOnPitfall)
            {
                yield return new WaitForSecondsRealtime(.6f);
                transform.Position2D(dynamicRespawnPos);
                AfterPitfall();
                transform.LocalScale2D(Vector2.one);
            }
            else
            {
                AfterPitfall();
                DestroySelf();
            }
        }

        /// <summary>
        /// This falling coroutine takes in an arry of custom respawn points and selects the safe one
        /// AFTER the falling animation has taken place.  This is done in case the respawn location moves
        /// over the course of the fall to ensure it doesn't respawn inside a wall.
        /// </summary>
        /// <param name="respawnLocationTransform"></param>
        /// <param name="tilemaps"></param>
        /// <returns></returns>
        private IEnumerator FallingCo(Transform[] respawnLocationTransform, List<Tilemap> tilemaps)
        {
            Vector2 scaleReduction = new Vector2(pitfallAnimSpeed, pitfallAnimSpeed);
            while (transform.LocalScale2D().x > 0)
            {
                transform.LocalScale2D(transform.LocalScale2D() - scaleReduction);
                yield return new WaitForSeconds(.01f);
            }
            yield return new WaitForSecondsRealtime(.1f);

            DuringPitfall();

            //TODO: NEED TO CLEAN ALL OF THIS SHIT UP, VERY UGLY ATM
            if (!isDestroyedOnPitfall)
            {
                yield return new WaitForSecondsRealtime(.6f);
                List<Transform> respawnFriendlyPoints = new List<Transform>();
                while (respawnFriendlyPoints.Count == 0)
                {
                    //check the pitfall locations for a safe area to respawn
                    //This is where it performs the pitfall calculation
                    //Removing all tilemaps from pitfall that are inside
                    //walls and other undesirable locations
                    foreach (var respawnPos in respawnLocationTransform)
                    {
                        if (IsRespawnFriendlyMap(respawnPos.transform.Position2D(), tilemaps))
                        {
                            //Double check that there's enough space for the obj to respawn
                            Collider2D[] colliders = Physics2D.OverlapCircleAll(respawnPos.transform.Position2D(), .5f);

                            bool addPoint = true;
                            foreach (var collider in colliders)
                            {
                                if (LayerMask.LayerToName(collider.gameObject.layer) == Constants.WALL ||
                                    LayerMask.LayerToName(collider.gameObject.layer) == Constants.OBJECT)
                                {

                                    if (collider?.attachedRigidbody?.GetComponent<RespawnData>() == null)
                                    {
                                        addPoint = false;
                                    }
                                }
                            }

                            if (addPoint) respawnFriendlyPoints.Add(respawnPos);
                        }
                        else
                        {
                            Debug.Log("Filtering out unsafe respawn point " + respawnPos.name + "Room: " + respawnPos?.parent?.name);
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
                    respawnLocation = GetRespawnPoint(respawnFriendlyPoints, pitfallPosition);
                }
                else
                {
                    respawnLocation = GetRespawnPoint(respawnFriendlyPoints, transform.Position2D());
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

        //FROM HERE DOWN I NEED TO CLEAN EVERYTHING UP A LOT!!!!!!!!!!!!!!!!!!!!

        //TODO I DON'T LIKE THIS METHOD, CLEAN UP
        private bool IsRespawnFriendlyMap(Vector2 worldPosition, List<Tilemap> tileMaps)
        {
            List<Tilemap> mapsAtWorldPosition = GetAllMapsWithTileAtPos(worldPosition, tileMaps);
            if (mapsAtWorldPosition.Count == 0)
            {
                return false;
            }

            if (!TryGetLargestSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayers, out int largestLayer))
            {
                return true;
            }

            //Checks if the map is respawn friendly
            foreach (var value in Dictionaries.RespawnFriendlySortingLayers.Values)
            {
                if (value == largestLayer) return true;
            }

            return false;
        }

        //TODO: I DON'T LIKE THIS ONE EITHER > In FRISBROS IT'S USED IN DISC AS WELL SO WEIRD TO MAKE IT PRIVATE HERE
        private Transform GetRespawnPoint(List<Transform> pitfallRespawnPoints, Vector2 pitfallTargetPos)
        {
            if (pitfallRespawnPoints.Count <= 0)
            {
                Debug.LogError("No respawn points supplied");
                return null;
            }

            Transform closestRespawn = pitfallRespawnPoints[0];
            foreach (var respawn in pitfallRespawnPoints)
            {
                if (Vector2.Distance(respawn.Position2D(), pitfallTargetPos) <
                    Vector2.Distance(closestRespawn.Position2D(), pitfallTargetPos))
                {
                    closestRespawn = respawn;
                }
            }
            return closestRespawn;
        }

        //TODO DON'T LIKE THAT It'S PRIVATE HERE
        private List<Tilemap> GetAllMapsWithTileAtPos(Vector2 worldPosition, List<Tilemap> tileMaps)
        {
            List<Tilemap> mapsAtWorldPosition = new List<Tilemap>();
            foreach (var map in tileMaps)
            {
                //gets the tilebase of every single map 
                Vector3Int gridPos = map.WorldToCell(worldPosition);
                TileBase objOnTile = map.GetTile(gridPos);
                if (objOnTile == null) continue;
                mapsAtWorldPosition.Add(map);
            }
            return mapsAtWorldPosition;
        }

        //TODO: DON'T LIKE THIS ONE EITHER
        private static bool TryGetLargestSortLayerAtPos(List<Tilemap> mapsAtWorldPosition, ReadOnlyDictionary<string, int> sortingLayers, out int sortingMapLayer)
        {
            //Removes tilemaps that are not on the sorting layers dict
            for (int i = 0; i < mapsAtWorldPosition.Count; i++)
            {
                string sortingName = mapsAtWorldPosition[i].GetComponent<TilemapRenderer>().sortingLayerName;
                if (!sortingLayers.ContainsKey(sortingName))
                {
                    //Debug.Log("Removing map: " + mapsAtWorldPosition[i].name + " because it's not in sortingLayers.");
                    mapsAtWorldPosition.Remove(mapsAtWorldPosition[i]);
                }
            }

            if (mapsAtWorldPosition.Count == 0)
            {
                sortingMapLayer = -1;
                return false;
            }

            //Initialize largest value collectors
            int surfaceMapLayer = sortingLayers[mapsAtWorldPosition[0].GetComponent<TilemapRenderer>().sortingLayerName];
            foreach (var map in mapsAtWorldPosition)
            {
                string sortingLayerName = map.GetComponent<TilemapRenderer>().sortingLayerName;
                //gets the greatest sorting layer of maps you are standing on
                int newMapSortingLayer;
                if (!sortingLayers.TryGetValue(sortingLayerName, out newMapSortingLayer))
                {
                    Debug.LogError(sortingLayerName);
                    sortingMapLayer = -1;
                    return false;
                }

                if (newMapSortingLayer > surfaceMapLayer)
                {
                    surfaceMapLayer = newMapSortingLayer;
                }
            }
            sortingMapLayer = surfaceMapLayer;
            return true;
        }
    }
}