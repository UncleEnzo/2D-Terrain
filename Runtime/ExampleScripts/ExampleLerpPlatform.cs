using Nevelson.Utils;
using UnityEngine;

namespace Nevelson.Terrain
{
    public class ExampleLerpPlatform : MonoBehaviour
    {
        private Vector2 destination;
        private Vector2 startPoint;
        private bool goRight = true;
        float timeElapsed = 0;
        float lerpDuration = 3;

        private void Start()
        {
            startPoint = transform.Position2D();
            destination = transform.Position2D() + (Vector2.right * 10);
        }

        void Update()
        {
            if (timeElapsed > lerpDuration)
            {
                goRight = !goRight;
                timeElapsed = 0;
            }

            Vector2 nextPos;
            if (goRight)
            {
                nextPos = Vector2.Lerp(startPoint, destination, timeElapsed / lerpDuration);
            }
            else
            {
                nextPos = Vector2.Lerp(destination, startPoint, timeElapsed / lerpDuration);
            }

            transform.Position2D(nextPos);
            timeElapsed += Time.deltaTime;
        }
    }
}