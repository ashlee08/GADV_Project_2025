using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private void Awake()
    {
        _edgeColl = GetComponent<EdgeCollider2D>();
        _boxColl = GetComponent<BoxCollider2D>();

        _water = GetComponent<InteractableWater>();
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
            }
        }

        //if (other.CompareTag("Rubbish"))
        //{
        //    if (!floatingTransforms.Contains(other.transform))
        //        floatingTransforms.Add(other.transform);
        //}
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if(other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().inWater = true;
        }

        // keep track of floating items on water surface
        if (rb != null && !floatingBodies.Contains(rb))
        {
            floatingBodies.Add(rb);
        }

        //if (other.CompareTag("Rubbish"))
        //{
        //    if (!floatingTransforms.Contains(other.transform))
        //        floatingTransforms.Add(other.transform);
        //}
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

        //if (other.CompareTag("Rubbish") && floatingTransforms.Contains(other.transform))
        //{
        //       floatingTransforms.Remove(other.transform);
        //}
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
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
