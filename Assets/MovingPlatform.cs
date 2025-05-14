using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public Direction moveDirection;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public float pauseTime = 1f;          // Pause duration at ends
    public bool moveToTarget = true;      // Toggle this to start/stop movement
    public bool loopMovement = true;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isPaused = false;
    private float pauseTimer = 0f;

    void Start()
    {
        startPosition = transform.position;

        switch (moveDirection)
        {
            case Direction.Up:
                targetPosition = startPosition + Vector3.up * moveDistance; // (3,4) + (0,1) * d
                break;
            case Direction.Down:
                targetPosition = startPosition + Vector3.down * moveDistance;
                break;
            case Direction.Left:
                targetPosition = startPosition + Vector3.left * moveDistance;
                break;
            case Direction.Right:
                targetPosition = startPosition + Vector3.right * moveDistance;
                break;
        }
    }

    void Update()
    {

        // If movement is disabled or paused, count down the pause timer
        if (isPaused)
        {
            if (!loopMovement)
            {
                return; // will not run code below to unpause;
            }

            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                moveToTarget = !moveToTarget; // Reverse direction after pause
            }
            return;
        }


        // Move toward the current destination
        Vector3 destination = moveToTarget ? targetPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        // Check if we've reached the destination
        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            isPaused = true;
            pauseTimer = pauseTime;
            transform.position = destination;
        }
    }
}
