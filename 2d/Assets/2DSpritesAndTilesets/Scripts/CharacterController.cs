using UnityEngine;

namespace NimbleGames.BackgroundAssets
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f; // Movement speed
        private Vector2 moveDirection; // Movement direction
        [SerializeField] private bool autoMove = false; // Boolean parameter to enable automatic movement in a given direction
        [SerializeField] private MovementDirection direction = MovementDirection.Right; // Direction of automatic movement
        private SpriteRenderer spriteRenderer; // Sprite renderer component

        // Enumeration to define the possible movement directions
        public enum MovementDirection
        {
            Top,
            Down,
            Left,
            Right
        }

        private void Start()
        {
            // Get the sprite renderer component
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // If autoMove parameter is enabled, set the movement direction to the chosen direction and disable the sprite renderer
            if (autoMove)
            {
                switch (direction)
                {
                    case MovementDirection.Top:
                        moveDirection = Vector2.up;
                        break;
                    case MovementDirection.Down:
                        moveDirection = Vector2.down;
                        break;
                    case MovementDirection.Left:
                        moveDirection = Vector2.left;
                        break;
                    case MovementDirection.Right:
                        moveDirection = Vector2.right;
                        break;
                }
                spriteRenderer.enabled = false;
            }
            else // Otherwise, get input for movement direction as usual and enable the sprite renderer
            {
                moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
                spriteRenderer.enabled = true;
            }
        }

        private void FixedUpdate()
        {
            // Move the character based on the movement direction and speed
            GetComponent<Rigidbody2D>().MovePosition(GetComponent<Rigidbody2D>().position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
