using UnityEngine;

namespace Nevelson.Terrain
{
    /// <summary>
    /// Example of how to implement controller movement with terrain system.
    /// Extend ControllerMovement to any movement script and implement abstract method, SetMoveInput
    /// This allows you to use any movement system you prefer to use, including Unity Input or Rewired
    /// </summary>
    public class ExampleUnityInputControllerMove : ControllerMovement
    {
        protected override Vector2 SetMoveInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }
}