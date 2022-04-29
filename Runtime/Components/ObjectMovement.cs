using UnityEngine;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class ObjectMovement : MovementBase
    {
        protected Rigidbody2D rb;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate()
        {
            TraverseTile(rigidBody.velocity);
        }
    }
}