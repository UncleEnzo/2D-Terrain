using UnityEngine;

namespace Nevelson.Terrain
{
    [System.Serializable]
    public struct TileSound
    {
        /// <summary>
        /// Useful for integration with FMOD that relies on file paths
        /// </summary>
        public string soundEventPath;
        /// <summary>
        /// Used for Unity Audio system
        /// </summary>
        public AudioClip audioClip;
    }
}