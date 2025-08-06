using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
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
    public float climbSpeed = 3f;
    public bool isClimbing = false;

    public bool inWater = false;
    public bool gameEnd = false;

    public Animator animator;
    float horizontalMove = 0f;

    public Vector3 initialPosition;
    private float snappedX;

    [Header("Sound Audio")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip loseSound;
    public AudioClip flameSound; // when someone touch the fire.
    public GameCamera gameCamera; // Reference to the GameCamera script

    // Start is called before the first frame update
    void Start()
    {
        // Take the ridgidBody 2d of game object with PlayerMovement script, and initialize Rb variable.
        Rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1f;
        gameEnd = false; // Initialize gameEnd to false
    }

    public void playerBurnt()
    {
        audioSource.PlayOneShot(flameSound); // Play the flame sound when player is burnt
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(0.474f, 0.474f, 0.471f); // Burnt color
    }

    public void loseGame()
    {
        gameCamera.StopBGM(); // Stop the background music
        gameEnd = true; // Set gameEnd to true
        audioSource.volume = 0.1f; // Ensure volume is set to 1 for the lose sound
        audioSource.PlayOneShot(loseSound);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // TO BE REMOVED, debugging purpose.
        //if (Input.GetKey(KeyCode.R))
        //{
        //    transform.position = initialPosition;
        //}
        if (gameEnd)
        {
            animator.SetBool("IsJumping", false);
            animator.SetFloat("Speed", 0f);
            Rb.velocity = Vector2.zero; // Stop all movement
            return; // If game has ended, do not process any movement or actions.
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
            if (audioSource.clip != jumpSound)
            {
                audioSource.Stop();
            }
            if (!audioSource.isPlaying)
            {
                audioSource.clip = jumpSound;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        if (inWater)
        {
            Rb.velocity = Vector2.zero; // Stop all movement in water
        }
        else if (isClimbing)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            Rb.gravityScale = 0f;
            // Lock horizontal movement and snap to ladder's X
            transform.position = new Vector3(snappedX, transform.position.y, transform.position.z);

            Rb.velocity = new Vector2(0f, vertical * climbSpeed);
        }
        else
        {
            Rb.gravityScale = 1f; // Ensure gravity is restored
        }

        // Animation
        horizontalMove = Input.GetAxisRaw("Horizontal") * playerSpeed;


        animator.SetBool("IsJumping", (!onGround || inWater || isClimbing));
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Run_Anim"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (!stateInfo.IsName("Run_Anim"))
        {
            if (audioSource.isPlaying)
            {
                audioSource.loop = false;
            }
        }
        
    }

    private void OnDrawGizmos()
    {
        // This is to show the ray cast down. Can comment away later. Debugging view.
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundLine);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            if (Input.GetKey(KeyCode.W))
            {
                isClimbing = true;

                // Snap player to the center X of the ladder
                snappedX = other.bounds.center.x;
                transform.position = new Vector3(snappedX, transform.position.y, transform.position.z);
                Rb.velocity = Vector2.zero;
            }

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }
}
