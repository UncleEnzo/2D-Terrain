using UnityEngine;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(AudioSource))]
    public class ExampleObjectMovement : ObjectMovement, IPitfallStates
    {
        [SerializeField] private AudioClip fallingSound;
        private RigidbodyConstraints2D contrain = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        private RigidbodyConstraints2D unconstrained = RigidbodyConstraints2D.None;
        private AudioSource audioSource;

        protected override void Start()
        {
            base.Start();
            audioSource = GetComponent<AudioSource>();
        }

        public void PF_Before()
        {
            rigidBody.constraints = contrain;
        }

        public void PF_During()
        {
            audioSource.PlayOneShot(fallingSound);
        }

        public void PF_After()
        {
            rigidBody.constraints = unconstrained;
        }
    }
}