using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nevelson.Terrain
{
    public class ObjectMovement_RuntimeTests
    {
        [UnityTest]
        public IEnumerator Test_WhileSceneLoaded()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            GameObject.Destroy(player);
            yield return null;
        }
        //Add more...
    }
}