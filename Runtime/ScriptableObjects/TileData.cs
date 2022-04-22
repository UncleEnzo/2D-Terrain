using Nevelson.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Nevelson.Terrain.Enums;
using static Nevelson.Utils.Enums;

namespace Nevelson.Terrain
{
    [CreateAssetMenu(fileName = "TileDataSO", menuName = "TerrainTile/TileDataSO")]
    public class TileData : ScriptableObject
    {
        public TileBase[] tileset = new TileBase[0];

        [Header("Percentage to increase or decrease object speed on tile")]
        [Range(-2f, 2f)] public float speedModifier = 1f;

        [Header("Directions to apply speed modifier")]
        public Direction[] applySpeedModInDir = new Direction[4] {
            Direction.LEFT,
            Direction.RIGHT,
            Direction.UP,
            Direction.DOWN
        };

        [Header("Switch between transform and rb")]
        public MovementType currentMovementType = MovementType.TRANSFORM;

        [Header("Percentage speed reduction on switching to physics-based movement")]
        [Range(0, 1)] public float onChangeToPhysicsSpeedReduction = .35f;

        [Header("Rigidbody velocity magnitude must be below this threshold to regain transform movement control after knockback")]
        [Range(0, 1)] public float knockbackRegainTransformControlThreshold = .35f;

        [Header("Order of dirs in array matters for diagonals")]
        public Direction[] conveyorBeltDir = new Direction[0];
        [Range(-300, 300)] public int conveyorSpeed = 100;
        [Range(0, .45f)] float tileYOffset = .4f;

        void OnValidate()
        {
            if (applySpeedModInDir.Length != applySpeedModInDir.Distinct().Count())
            {
                Debug.LogError("TileData dirsToSpeedMod array contains duplicates.");
            }

            if (applySpeedModInDir.Length > 4)
            {
                Debug.LogWarning("Don’t increase array size past four! Resizing...");
                Array.Resize(ref applySpeedModInDir, 4);
            }

            if (conveyorBeltDir.Length > 2)
            {
                Debug.LogWarning("Don't increase array size past two!");
                Array.Resize(ref conveyorBeltDir, 2);
            }
        }


        public MovementType ApplyTileProperties(Rigidbody2D rigidbody, Vector2 moveVelocity, MovementType currentMovementType, Vector2 tilePos)
        {
            Vector2 moveVelocitySpeedAdjusted = ModifySpeedInDir(moveVelocity);
            moveVelocitySpeedAdjusted = ModifyConveyorBeltInDir(moveVelocitySpeedAdjusted, tilePos, rigidbody);
            HandleMovement(rigidbody, moveVelocitySpeedAdjusted, currentMovementType);
            return this.currentMovementType;
        }

        private void HandleMovement(Rigidbody2D rigidbody, Vector2 moveVelocity, MovementType previousMovementType)
        {
            OnChange_HandleMovementTypeTransition(rigidbody, moveVelocity, previousMovementType);
            ApplyMovementToRigidBody(rigidbody, moveVelocity);
        }

        private void ApplyMovementToRigidBody(Rigidbody2D rigidbody, Vector2 moveVelocity)
        {
            if (currentMovementType == MovementType.TRANSFORM)
            {
                //EXPLAIN THIS
                if (rigidbody.velocity.magnitude < knockbackRegainTransformControlThreshold)
                {
                    rigidbody.MovePosition(rigidbody.position + moveVelocity * Time.fixedDeltaTime);
                }
            }
            else
            {
                rigidbody.AddForce(moveVelocity, ForceMode2D.Force);
            }
        }

        private void OnChange_HandleMovementTypeTransition(Rigidbody2D rigidbody, Vector2 moveVelocity, MovementType lastMovementType)
        {
            //Switching Physics to Transform: Reset velocity to prevent
            // object from shooting off at high speeds on first frame
            if (currentMovementType == MovementType.TRANSFORM && lastMovementType != currentMovementType)
            {
                rigidbody.velocity = Vector2.zero;
            }
            //Switching Transform to Physics:  Lower Move Velocity to
            //prevent high momentum build up from physics on first frame
            else if (currentMovementType != lastMovementType)
            {
                rigidbody.velocity = moveVelocity * onChangeToPhysicsSpeedReduction;
            }
        }

        private Vector2 ModifySpeedInDir(Vector2 moveVelocity)
        {
            foreach (Direction dir in applySpeedModInDir)
            {
                switch (dir)
                {
                    case Direction.LEFT:
                        if (moveVelocity.x < 0) return ModifySpeed(moveVelocity);
                        break;
                    case Direction.RIGHT:
                        if (moveVelocity.x > 0) return ModifySpeed(moveVelocity);
                        break;
                    case Direction.UP:
                        if (moveVelocity.y > 0) return ModifySpeed(moveVelocity);
                        break;
                    case Direction.DOWN:
                        if (moveVelocity.y < 0) return ModifySpeed(moveVelocity);
                        break;
                }
            }
            return moveVelocity;
        }

        private Vector2 ModifySpeed(Vector2 moveVelocity)
        {
            return moveVelocity *= speedModifier;
        }

        private Vector2 ModifyConveyorBeltInDir(Vector2 moveVelocity, Vector2 tilePos, Rigidbody2D rb)
        {
            if (conveyorBeltDir.Length <= 0) return moveVelocity;
            float beltVelocity = conveyorSpeed * Time.deltaTime;

            if (conveyorBeltDir.Length > 1)
            {
                Vector2 speedOne = beltVelocity * RODicts.DirectionVectorCardinal[conveyorBeltDir[0]];
                Vector2 speedTwo = beltVelocity * RODicts.DirectionVectorCardinal[conveyorBeltDir[1]];
                switch (conveyorBeltDir[0])
                {
                    case Direction.LEFT:
                        if (rb.transform.Position2D().x > tilePos.x)
                        {
                            moveVelocity += speedOne;
                        }
                        else
                        {
                            moveVelocity += speedTwo;
                        }
                        break;
                    case Direction.RIGHT:
                        if (rb.transform.Position2D().x < tilePos.x)
                        {
                            moveVelocity += speedOne;
                        }
                        else
                        {
                            moveVelocity += speedTwo;
                        }
                        break;
                    case Direction.UP:
                        if (rb.transform.Position2D().y < tilePos.y - tileYOffset)
                        {
                            moveVelocity += speedOne;
                        }
                        else
                        {
                            moveVelocity += speedTwo;
                        }
                        break;
                    case Direction.DOWN:
                        if (rb.transform.Position2D().y > tilePos.y - tileYOffset)
                        {
                            moveVelocity += speedOne;
                        }
                        else
                        {
                            moveVelocity += speedTwo;
                        }
                        break;
                }
            }
            else
            {
                Vector2 speed = beltVelocity * RODicts.DirectionVector[conveyorBeltDir[0]];
                moveVelocity += speed;
            }

            return moveVelocity;
        }
    }
}