namespace Nevelson.Terrain
{
    public class ObjectMovement : MovementBase, IPitfallCondition, IPitfallStates
    {
        //TODO: NOTE ON THESE : THEY CAN BE IMPLEMENTED ANYWHERE ON THE GAME OBJECT,
        //DOES NOT HAVE TO BE ON THE MOVEMENT SCRIPT (PROBABLY SHOULDN"T)
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

        private void FixedUpdate()
        {
            TraverseTile(rigidBody.velocity);
        }
    }
}