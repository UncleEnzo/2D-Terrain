using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ExampleRBMovePlatform : MonoBehaviour
    {
        private Rigidbody2D rigidBody;
        private Coroutine co = null;
        private bool isGoingLeft = false;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (co == null)
            {
                co = StartCoroutine(MoveHorizontal(isGoingLeft ? Vector2.left : Vector2.right));
                isGoingLeft = !isGoingLeft;
            }
        }


        private IEnumerator MoveHorizontal(Vector2 dir)
        {
            int frames = 1000;
            while (frames > 0)
            {
                rigidBody.AddForce(dir, ForceMode2D.Force);
                frames--;
                yield return null;
            }
            co = null;
        }
    }
}