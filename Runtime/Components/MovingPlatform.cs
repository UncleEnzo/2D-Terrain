using Nevelson.Utils;
using UnityEngine;
using static Nevelson.Terrain.Enums;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(TilemapSubscriber))]
    public class MovingPlatform : MonoBehaviour, IMovingPlatform
    {
        public Vector2 MoveVelocity { get => moveVelocity; }
        public MovementType movingPlatformMoveType = MovementType.TRANSFORM;

        private Rigidbody2D rb;
        private Vector2 previousPosition = Vector2.zero;
        private Vector2 moveVelocity = Vector2.zero;

        protected virtual void Update()
        {
            switch (movingPlatformMoveType)
            {
                case MovementType.TRANSFORM:
                    if (previousPosition == Vector2.zero)
                    {
                        previousPosition = transform.Position2D();
                        moveVelocity = Vector2.zero;
                        return;
                    }
                    moveVelocity = previousPosition.GetDirection(transform.Position2D()) / Time.deltaTime;
                    break;
                case MovementType.PHYSICS:
                    if (rb == null)
                    {
                        rb = GetComponent<Rigidbody2D>();
                    }
                    moveVelocity = rb.velocity;
                    break;
                default:
                    Debug.LogWarning("Movement type does not exist");
                    break;
            }
            previousPosition = transform.Position2D();
        }

        protected virtual void FixedUpdate() { }
    }
}