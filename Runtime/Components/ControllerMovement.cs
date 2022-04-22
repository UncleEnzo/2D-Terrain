using UnityEngine;

namespace Nevelson.Terrain
{
    public class ControllerMovement : MovementBase, IPitfallCondition, IPitfallStates
    {
        public Vector2 MoveVelocity { get; set; }
        [SerializeField] private float defaultMoveSpeed = 7f;

        //TODO: NOTE ON THESE : THEY CAN BE IMPLEMENTED ANYWHERE ON THE GAME OBJECT,
        //DOES NOT HAVE TO BE ON THE MOVEMENT SCRIPT (PROBABLY SHOULDN"T
        //SHOULD THIS BE UNITY EVENTS SO IT'S SERIALIZABLE??
        public bool PF_Check()
        {
            throw new System.NotImplementedException();
        }

        public void PF_Before()
        {
            throw new System.NotImplementedException();
        }

        public void PF_During()
        {
            throw new System.NotImplementedException();
        }

        public void PF_After()
        {
            throw new System.NotImplementedException();
        }

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
            TraverseTile(MoveVelocity);
        }

        private void SetMovementVelocity(Vector2 moveInput)
        {
            MoveVelocity = moveInput * defaultMoveSpeed;
        }
    }
}