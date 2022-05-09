using UnityEngine;

namespace Nevelson.Terrain
{
    public class CameraConfig : MonoBehaviour
    {
        Camera cam => GetComponent<Camera>();
        private const float pixelPerfectx16 = 8.534091f;

        void Start()
        {
            cam.orthographicSize = pixelPerfectx16;
            cam.allowMSAA = false;
        }

        void Update()
        {
            if (cam.orthographicSize != pixelPerfectx16)
            {
                cam.orthographicSize = pixelPerfectx16;
            }
        }
    }
}