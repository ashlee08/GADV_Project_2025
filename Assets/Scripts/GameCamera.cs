using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    [Header("Camera X Bounds")]
    public float minX = -10f;
    public float maxX = 10f;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Clamp X so the camera doesn't go outside the desired range
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);

            // Force Z so camera stays in correct layer
            smoothedPosition.z = -10;

            transform.position = smoothedPosition;
        }
    }
}
