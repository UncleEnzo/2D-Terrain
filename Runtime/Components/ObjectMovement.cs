namespace Nevelson.Terrain
{
    public class ObjectMovement : MovementBase
    {
        private void FixedUpdate()
        {
            TraverseTile(transform.position, rigidBody, rigidBody.velocity);
        }
    }
}