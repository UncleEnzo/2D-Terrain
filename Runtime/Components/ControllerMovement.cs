using UnityEngine;

namespace Nevelson.Terrain
{
    public abstract class ControllerMovement : MovementBase
    {
        public Vector2 MoveVelocity { get; private set; }
        [SerializeField] private float defaultMoveSpeed = 7f;

        protected virtual void FixedUpdate()
        {
            Vector2 moveInput = SetMoveInput();
            SetMovementVelocity(moveInput);
            TraverseTile(MoveVelocity);
        }

        protected abstract Vector2 SetMoveInput();

        private void SetMovementVelocity(Vector2 moveInput)
        {
            MoveVelocity = moveInput * defaultMoveSpeed;
        }
    }
}