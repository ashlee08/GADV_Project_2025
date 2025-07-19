using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D Rb;
    public LayerMask groundLayer;

    [Header("Stats")]
    public float playerSpeed;
    public float playerJumpForce;

    [Header("Collision")] // optional, just look nicer in unity editor.
    public bool onGround = false;
    public float groundLine;

    public bool inWater = false;
    public bool playerLose = false;

    public Animator animator;
    float horizontalMove = 0f;

    public Vector3 initialPosition;


    // Start is called before the first frame update
    void Start()
    {
        // Take the ridgidBody 2d of game object with PlayerMovement script, and initialize Rb variable.
        Rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // TO BE REMOVED, debugging purpose.
        if (Input.GetKey(KeyCode.R))
        {
            transform.position = initialPosition;
        }

        // from the player position, casting downward to find a collision with groundLayer within distance groundLine.
        onGround = Physics2D.Raycast(transform.position, Vector2.down, groundLine, groundLayer);
        bool space = Input.GetKey(KeyCode.Space);// check if anyone press onto spacebar
        // and (&&), or (||)
        bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        bool left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        if (left || right)
        {
            int direction = 0;
            //  (left) ? -1 : 0; <- ternary operator, shortform of if else.
            direction += (left) ? -1 : 0;
            direction += (right) ? 1 : 0;
            if (direction > 0)
            {
                transform.localScale = new Vector3(1, 1, 1); // Face right
            }
            else if (direction < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            // left is true and right is false, direction = -1
            // left is false and right is true, direction = 1
            // left is true and right is true, direction = 0

            // Add a X force to -1*playerSpeed
            Rb.velocity = new Vector2(direction * playerSpeed, Rb.velocity.y);
        }
        else
        {
            Rb.velocity = new Vector2(0f, Rb.velocity.y);
        }

        if (space && onGround)
        {
            // Rb.AddForce(new Vector2(0, playerJumpForce), ForceMode2D.Impulse);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.11f, transform.position.z);
            Rb.velocity = new Vector2(Rb.velocity.x, playerJumpForce);
        }

        if (inWater)
        {
            Rb.velocity = Vector2.zero; // Stop all movement in water
        }

        // Animation
        horizontalMove = Input.GetAxisRaw("Horizontal") * playerSpeed;
        

        animator.SetBool("IsJumping", (!onGround || inWater));
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
    }

    private void OnDrawGizmos()
    {
        // This is to show the ray cast down. Can comment away later. Debugging view.
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundLine);
    }
  

}
