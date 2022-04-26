using UnityEngine;

namespace Nevelson.Terrain
{
    public interface IMovingPlatform
    {
        Vector2 MoveVelocity
        {
            get;
        }
    }
}