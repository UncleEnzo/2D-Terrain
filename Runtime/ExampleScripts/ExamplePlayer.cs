using UnityEngine;

namespace Nevelson.Terrain
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ExampleUnityInputControllerMove))]
    public class ExamplePlayer : MonoBehaviour, IPitfallCondition, IPitfallStates, IInteractTiles
    {
        public string InteractTooltip { get => interactString; }

        [Header("Example condition where player may not fall into pit")]
        [SerializeField] private bool isHovering = false;
        [Header("Takes damage on pitfall")]
        [SerializeField] private int health = 10;

        [SerializeField] TileData pitfallInteract;
        [SerializeField] TileData interactableTile1;
        [SerializeField] TileData interactableTile2;

        private Animator anim;
        private SpriteRenderer spriteRenderer;
        private ExampleUnityInputControllerMove movement;
        private bool isSpriteFlipped = false;
        private string interactString = "";

        private void Start()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            anim = GetComponentInChildren<Animator>();
            movement = GetComponent<ExampleUnityInputControllerMove>();
        }

        private void Update()
        {
            Vector2 inputRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            SetSpriteRendererDirection(inputRaw.x);
            SetAnimationState(inputRaw);
            FocusCameraOnPlayer();
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

        private void FocusCameraOnPlayer()
        {
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
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
            //Examples:
            //Audio Effects from AudioSource or other player
            //Water Splash effect
        }

        public void PF_After()
        {
            movement.enabled = true;
        }

        public void InteractWithTile(TileData tileData, bool isTileInteractable)
        {
            if (!isTileInteractable)
            {
                interactString = "";
                return;
            }

            if (tileData == pitfallInteract)
            {
                interactString = "Press Space";
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Treading Water!");
                }
            }

            if (tileData == interactableTile1)
            {
                interactString = "Press Space";
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("InteractTileOne Interaction Triggered!");
                }
            }

            if (tileData == interactableTile2)
            {
                interactString = "Press Space";
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("I pressed space on interaction tile 2!");
                }
            }
        }
    }
}