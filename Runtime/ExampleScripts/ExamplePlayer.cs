using UnityEngine;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(ExampleUnityInputControllerMove))]
    public class ExamplePlayer : MonoBehaviour, IPitfallCondition, IPitfallStates
    {
        [Header("Example condition where player may not fall into pit")]
        [SerializeField] private bool isHovering = false;
        [Header("Takes damage on pitfall")]
        [SerializeField] private int health = 10;
        [Header("Takes damage on pitfall")]
        [SerializeField] private AudioClip fallingSound;
        [SerializeField] private AudioClip hitSound;

        private Animator anim;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private ExampleUnityInputControllerMove movement;
        private bool isSpriteFlipped = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            anim = GetComponentInChildren<Animator>();
            movement = GetComponent<ExampleUnityInputControllerMove>();
        }

        private void Update()
        {
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

        public bool PF_Check()
        {
            return !isHovering;
        }

        public void PF_Before()
        {
            movement.enabled = false;
        }

        public void PF_During()
        {
            health--;
            audioSource.PlayOneShot(fallingSound);
        }

        public void PF_After()
        {
            audioSource.PlayOneShot(hitSound);
            movement.enabled = true;
        }
    }
}