using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        private Animator anim;
        private SpriteRenderer spriteRenderer;
        private bool isSpriteFlipped = false;


        private void Start()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            anim = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            //USED FOR TESTING, REMOVE LATER
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SceneManager.LoadScene(0);
            }

            Vector2 inputRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SetSpriteRendererDirection(inputRaw.x);
            SetAnimationState(inputRaw);
        }

        private void SetSpriteRendererDirection(float inputRawX)
        {
            switch (inputRawX)
            {
                case 0:
                    spriteRenderer.flipX = isSpriteFlipped;
                    break;
                case -1:
                    isSpriteFlipped = true;
                    spriteRenderer.flipX = isSpriteFlipped;
                    break;
                case 1:
                    isSpriteFlipped = false;
                    spriteRenderer.flipX = isSpriteFlipped;
                    break;
            }
        }

        private void SetAnimationState(Vector2 inputRaw)
        {
            anim.SetBool("isMoving", inputRaw != Vector2.zero);
        }
    }
}