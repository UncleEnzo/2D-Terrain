using UnityEngine;

namespace Nevelson.Terrain
{
    public abstract class ControllerMovement : MovementBase
    {
        public Vector2 MoveVelocity { get; private set; }
        [SerializeField] private float defaultMoveSpeed = 7f;

        protected virtual void FixedUpdate()
        {
            Vector2 moveInput = SetTerrainMoveInput();
            SetMovementVelocity(moveInput);
            TraverseTile(MoveVelocity);
        }

        protected abstract Vector2 SetTerrainMoveInput();

        private void SetMovementVelocity(Vector2 moveInput)
        {
            MoveVelocity = moveInput * defaultMoveSpeed;
        }
    }
}