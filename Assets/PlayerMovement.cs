using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D Rb;

    [Header("Stats")]
    public float playerSpeed;
    public float playerJumpForce;
    // Start is called before the first frame update
    void Start()
    {
        // Take the ridgidBody 2d of game object with PlayerMovement script, and initialize Rb variable.
        Rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        bool space = Input.GetKey(KeyCode.Space);// check if anyone press onto spacebar
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        if (left)
        {
            // Add a X force to -1*playerSpeed
            Rb.AddForce(new Vector2(-playerSpeed, 0), ForceMode2D.Impulse);
        }
        if (right)
        {
            Rb.AddForce(new Vector2(playerSpeed, 0), ForceMode2D.Impulse);
        }
        if (space)
        {
            Rb.AddForce(new Vector2(0, playerJumpForce), ForceMode2D.Impulse);
        }
    }
}
