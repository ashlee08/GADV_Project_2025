using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectItem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rubbish"))
        {
            // prevent the rubbish on water to be picked up.
            BoxCollider2D bc = collision.gameObject.GetComponent<BoxCollider2D>();
            if (!bc.isTrigger)
            {
                Destroy(collision.gameObject);
            }
            
        }
    }

}
