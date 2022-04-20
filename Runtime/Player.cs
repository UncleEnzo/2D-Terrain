using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        //public float speed = 6f;
        //public Vector2 MoveVelocity { get; set; }
        //private Rigidbody2D rb;
        private Animator anim;
        private SpriteRenderer spriteRenderer;
        private bool isSpriteFlipped = false;


        private void Start()
        {
            //rb = GetComponent<Rigidbody2D>();
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
            //SetMovementVelocity(inputRaw);
            SetSpriteRendererDirection(inputRaw.x);
            SetAnimationState(inputRaw);
        }

        private void FixedUpdate()
        {
            //rb.MovePosition(new Vector2(transform.position.x, transform.position.y) + MoveVelocity * Time.fixedDeltaTime);
        }

        //private void SetMovementVelocity(Vector2 moveInput)
        //{
        //    MoveVelocity = moveInput * speed;
        //}

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