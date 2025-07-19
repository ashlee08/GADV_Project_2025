using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _waterMask;

    private EdgeCollider2D _edgeColl;
    private BoxCollider2D _boxColl;
    private InteractableWater _water;
    private List<Rigidbody2D> floatingBodies = new List<Rigidbody2D>();
    private List<Transform> floatingTransforms = new List<Transform>();
    public float bounyancyStrength = 100f;
    public float bobblingStrength = 0.5f;
    public PollutionBar pollution;
    public GameObject gameOverPanel;

    [Header("Rotation Effects")]
    [SerializeField] public float _rotationStrength = 0.5f;
    [SerializeField] public float _maxRotionForce = 2f;
    [SerializeField] public float _surfaceTorqueMultiplier = 1.5f;
    [SerializeField] private bool _randomizeRotationDirection = true;


    private void Awake()
    {
        _edgeColl = GetComponent<EdgeCollider2D>();
        _boxColl = GetComponent<BoxCollider2D>();

        _water = GetComponent<InteractableWater>();
    }

    IEnumerator MakeTriggerAfterDelay(Collider2D col)
    {
        yield return new WaitForFixedUpdate();
        if(col.isTrigger == false)
        {
            Logger.Equals("MakeTriggerAfterDelay", "Making trigger for " + col.name);
            pollution.AddPollution(1);
            col.isTrigger = true;
        }
        
    }

    private void ApplyRotationForce(Rigidbody2D rb, float force, bool isInitialImpact)
    {
        float direction = _randomizeRotationDirection ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(rb.velocity.x);
        float torqueForce;
        if (isInitialImpact)
        {
            // Apply a stronger force on initial impact
            torqueForce = Mathf.Min(force * _rotationStrength * _surfaceTorqueMultiplier, _maxRotionForce * 2f);
        }
        else
        {
            // Apply a normal force for continuous rotation
            torqueForce = Mathf.Min(force * _rotationStrength * 0.1f, _maxRotionForce);
        }
        rb.AddTorque(torqueForce * direction, ForceMode2D.Force);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if ((_waterMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (!floatingBodies.Contains(rb))
            {
                floatingBodies.Add(rb);
            }
            if (rb != null)
            {
                
                Vector2 localPos = gameObject.transform.localPosition;
                Vector2 hitObjectPos = other.transform.position;
                Bounds hitObjectBounds = other.bounds;
                Vector3 spawnPos = Vector3.zero;
                if(other.transform.position.y >= _edgeColl.points[1].y + _edgeColl.offset.y + localPos.y)
                {
                    spawnPos = hitObjectPos - new Vector2(0, hitObjectBounds.extents.y);
                }
                else
                {
                    spawnPos = hitObjectPos + new Vector2(0, hitObjectBounds.extents.y);
                }

                // clamp splash point to a MAX velocity
                int multiplier = (rb.velocity.y < 0)?-1:1;
                float vel = rb.velocity.y * _water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, _water.MaxForce);
                vel*= multiplier;
                _water.Splash(other, vel);
                // Appy rotation force
                ApplyRotationForce(rb, vel, true);
            }
        }

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().inWater = true;
            gameOverPanel.SetActive(true);
        }


        if(other.tag == "Rubbish" && other.isTrigger == false)
        {
            // a delay way to make sure set isTrigger to true after the collision
            // so that the rubbish cannot be picked up by player.
            StartCoroutine(MakeTriggerAfterDelay(other));
        }

        // keep track of floating items on water surface
        if (rb != null && !floatingBodies.Contains(rb))
        {
            floatingBodies.Add(rb);
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().inWater = false;
        }


        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null && floatingBodies.Contains(rb))
        {
            floatingBodies.Remove(rb);
        }

    }
    private void Update()
    {
        foreach (Transform t in floatingTransforms)
        {
            if (t == null) continue;

            // Keep them at water surface + bobbing
            float targetY = _boxColl.bounds.max.y;

            // Optional: Bobbing using sine wave
            float bob = Mathf.Sin(Time.time * 3f + t.position.x * 0.5f) * bobblingStrength;

            Vector3 pos = t.position;
            pos.y = Mathf.Lerp(pos.y, targetY + bob, Time.deltaTime * 3f); // smooth transition
            t.position = pos;
        }
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody2D rb in floatingBodies)
        {
            if (rb == null) continue;

            // Water surface height
            float waterSurfaceY = _boxColl.bounds.max.y;

            // Distance from surface to player center
            float depth = waterSurfaceY - rb.position.y;

            // Only float if slightly underwater (bonus tip)
            if (depth > 0.1f)
            {
                // 1. Buoyancy force (stronger the deeper you go)
                float buoyancyForce = Mathf.Clamp(depth * bounyancyStrength, 0f, bounyancyStrength*10);
                rb.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
            }

            // 2. Water drag to slow motion
            Vector2 drag = -rb.velocity * 2.5f;
            rb.AddForce(drag, ForceMode2D.Force);

            // 3. Bobbing effect (gentle sine wave motion)
            float bobAmount = Mathf.Sin(Time.time * 3f + rb.position.x * 0.5f) * bobblingStrength;
            rb.AddForce(Vector2.up * bobAmount, ForceMode2D.Force);
            //Debug.Log($"Inside Water: {depth}");

            if(Mathf.Abs(rb.angularVelocity) < _maxRotionForce * 2f)
            {
                ApplyRotationForce(rb, depth, false);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
