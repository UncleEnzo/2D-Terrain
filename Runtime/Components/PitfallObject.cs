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
        [Tooltip("Distance obj autorespawns from pit")]
        [SerializeField] private float respawnDistFromPit = 2;
        [Tooltip("Controls how quickly the falling animations player")]
        [SerializeField] private float pitfallAnimSpeed = .015f;
        [SerializeField] private bool isDestroyedOnPitfall = true;
        [Tooltip("Example unsafe layers > Wall, Object")]
        [SerializeField] private LayerMask unsafeRespawnLayers;


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

            switch (respawnMode)
            {
                case RespawnMode.AUTOMATIC:
                    //ToDo: Not implemented yet. I don't like how we CHOOSE the dynamic location.  need to make it a position the player was at X seconds agos
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
        //Edge cases to note:
        //  Tilemap moves or updates, what is the failsafe?
        //  Last location was equally dangerous
        //  Pitfalls immediately after other pitfall
        private IEnumerator AutomaticFallingCo()
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

        //TODO:
        //   -Clean this
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
                        if (IsRespawnFriendlyMap(respawnPos.transform.Position2D(), tilemaps))
                        {
                            //Double check there is enough space for the obj to respawn
                            Collider2D[] colliders = Physics2D.OverlapCircleAll(respawnPos.transform.Position2D(), .5f);
                            bool addPoint = true;
                            foreach (var collider in colliders)
                            {
                                if (unsafeRespawnLayers.IsInLayerMask(LayerMask.LayerToName(collider.gameObject.layer)))
                                {
                                    if (collider?.attachedRigidbody?.GetComponent<RespawnData>() == null)
                                    {
                                        addPoint = false;
                                    }
                                }
                            }
                            if (addPoint)
                            {
                                respawnFriendlyPoints.Add(respawnPos);
                            }
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

        private bool IsRespawnFriendlyMap(Vector2 worldPosition, List<Tilemap> tileMaps)
        {
            List<Tilemap> mapsAtWorldPosition = GetMapsAtPos(worldPosition, tileMaps);
            if (mapsAtWorldPosition.Count == 0)
            {
                return false;
            }

            if (!TryGetSortLayerAtPos(mapsAtWorldPosition, Dictionaries.SortingLayers, out int largestLayer))
            {
                return true;
            }

            //Checks if the map is respawn friendly
            foreach (var value in Dictionaries.RespawnFriendlySortingLayers.Values)
            {
                if (value == largestLayer)
                {
                    return true;
                }
            }

            return false;
        }
    }
}