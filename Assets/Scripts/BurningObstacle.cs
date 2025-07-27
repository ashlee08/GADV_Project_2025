using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningObstacle : MonoBehaviour
{
    public float onDuration = 2f;  // Time fire stays on
    public float offDuration = 1f; // Time fire stays off
    public float bounceForce = 10f;

    private bool fireActive = true;
    private float timer;
    private ParticleSystem fireParticles;

    void Start()
    {
        fireParticles = GetComponentInChildren<ParticleSystem>();
        timer = onDuration;
        SetFireState(true);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (fireActive && timer <= 0)
        {
            SetFireState(false);
            timer = offDuration;
        }
        else if (!fireActive && timer <= 0)
        {
            SetFireState(true);
            timer = onDuration;
        }
    }

    void SetFireState(bool isOn)
    {
        fireActive = isOn;

        if (fireParticles != null)
        {
            if (isOn)
                fireParticles.Play();
            else
                fireParticles.Stop();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (fireActive && other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                other.GetComponent<CapsuleCollider2D>().isTrigger = true;
                other.GetComponent<BoxCollider2D>().isTrigger = true;
                // Rotate player 90 degrees clockwise
                other.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }
}