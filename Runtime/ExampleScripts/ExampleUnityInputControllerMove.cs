using UnityEngine;

namespace Nevelson.Terrain
{
    /// <summary>
    /// Example of how to implement controller movement with terrain system.
    /// Extend ControllerMovement to any script that is responsible for movement
    /// Is compatible with other controller assets like Rewired.
    /// </summary>
    public class ExampleUnityInputControllerMove : ControllerMovement
    {
        protected override Vector2 SetMoveInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }
}