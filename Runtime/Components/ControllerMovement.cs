using UnityEngine;

namespace Nevelson.Terrain
{
    public class ControllerMovement : MovementBase
    {
        public Vector2 MoveVelocity { get; set; }
        public float defaultMoveSpeed = 7f;

        private void Update()
        {
            //TESTING STUFF
            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    GetComponent<Rigidbody2D>().AddForce(Vector2.right * 5, ForceMode2D.Impulse);
            //}
        }

        private void FixedUpdate()
        {
            Vector2 inputRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SetMovementVelocity(inputRaw);
            TraverseTile(transform.position, rigidBody, MoveVelocity);
        }

        private void SetMovementVelocity(Vector2 moveInput)
        {
            MoveVelocity = moveInput * defaultMoveSpeed;
        }
    }
}