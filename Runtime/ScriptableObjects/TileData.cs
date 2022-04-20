using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Nevelson.Terrain.Enums;
using static Nevelson.Utils.Enums;

namespace Nevelson.Terrain
{
    [CreateAssetMenu(fileName = "TileDataSO", menuName = "ScriptableObjects/TileDataSO")]
    public class TileData : ScriptableObject
    {
        public TileBase[] tileset;

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
        }


        public MovementType ApplyTileProperties(Rigidbody2D rigidbody, Vector2 moveVelocity, MovementType currentMovementType)
        {
            Vector2 moveVelocitySpeedAdjusted = ModifySpeedInDir(moveVelocity);
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
    }
}