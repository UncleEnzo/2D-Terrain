using UnityEngine;

namespace Nevelson.Terrain
{
    public class ExampleObjectMovement : ObjectMovement, IPitfallStates
    {
        [SerializeField] private AudioClip fallingSound;
        private RigidbodyConstraints2D contrain = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        private RigidbodyConstraints2D unconstrained = RigidbodyConstraints2D.None;

        protected override void Start()
        {
            base.Start();
        }

        public void PF_Before()
        {
            rigidBody.constraints = contrain;
        }

        public void PF_During()
        {
            //Examples:
            //Audio Effects from AudioSource or other player
            //Water Splash effect
        }

        public void PF_After()
        {
            rigidBody.constraints = unconstrained;
        }
    }
}