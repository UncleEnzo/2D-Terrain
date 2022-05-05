using UnityEngine;

namespace Nevelson.Terrain
{
    /// <summary>
    /// Example of how to implement controller movement with terrain system.
    /// Extend ControllerMovement to any movement script and implement abstract method, SetMoveInput
    /// This allows you to use any movement system you prefer to use, including Unity Input or Rewired
    /// </summary>
    public class ExampleUnityInputControllerMove : ControllerMovement, ITileSound
    {
        private bool triggerSound = false;
        private float soundTimer = 1f;
        private const float soundTimerReset = 1f;
        private Vector2 moveVelocity = Vector2.zero;

        protected override void Update()
        {
            base.Update();
            moveVelocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SoundTimer();
        }

        protected override Vector2 SetTerrainMoveInput()
        {
            return moveVelocity;
        }

        private void SoundTimer()
        {
            if (moveVelocity.magnitude == 0)
            {
                return;
            }

            if (soundTimer > 0)
            {
                soundTimer -= Time.deltaTime;
            }
            else
            {
                soundTimer = soundTimerReset;
                triggerSound = true;
            }
        }

        public void PlayTileSound(TileSound[] tileSounds, bool tileHasSound)
        {
            if (!tileHasSound)
            {
                return;
            }

            if (moveVelocity.magnitude == 0)
            {
                return;

            }

            if (triggerSound)
            {
                Debug.Log("Player move sound triggered");
            }
        }
    }
}