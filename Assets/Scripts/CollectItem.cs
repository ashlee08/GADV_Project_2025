using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Security.Cryptography.X509Certificates; // TextMeshPro namespace for UI text

public class CollectItem : MonoBehaviour
{
    private int pickupCount = 0; // Counter for picked up items
    private int totalItems = 0; // Total items in the scene
    // Reference to the UI TextMeshPro component to display the count
    public TextMeshProUGUI itemCountText;
    // Reference to the UI TextMeshPro component to display the total count
    public TextMeshProUGUI totalCountText;
    public GameObject winPanel;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    [Header("Sound Audio")]
    public AudioSource audioSource;
    public AudioClip pickUpSound;
    public AudioClip winSound;

    // Start is called before the first frame update
    void Start()
    {
        // Grab total item from Tag "Rubbish"
        GameObject[] items = GameObject.FindGameObjectsWithTag("Rubbish");
        totalItems = items.Length; // Count the total number of items
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (itemCountText != null) itemCountText.text = $"{pickupCount}";
        if (totalCountText != null) totalCountText.text = $"/{totalItems}";
    }

    // Attach to an object with a Collider2D set as isTrigger = true
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WinningDoor"))
        {

            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null && movement.gameEnd == false)
            {
                movement.gameEnd = true; // Set gameEnd to true
                audioSource.PlayOneShot(winSound);
                movement.gameCamera.StopBGM(); // Stop the background music
            }
            winPanel.SetActive(true);
            float percentage = (float)pickupCount / totalItems;
            if (percentage >= 1f)
            {
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
            }
            else if (percentage >= 0.75f)
            {
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(false);
            }
            else if (percentage >= 0.5f)
            {
                star1.SetActive(true);
                star2.SetActive(false);
                star3.SetActive(false);
            }
            else
            {
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rubbish"))
        {
            // prevent the rubbish on water to be picked up.
            BoxCollider2D bc = collision.gameObject.GetComponent<BoxCollider2D>();
            if (!bc.isTrigger)
            {
                // Increment the pickup count
                pickupCount++;
                itemCountText.text = $"{pickupCount}";
                Destroy(collision.gameObject);
                audioSource.PlayOneShot(pickUpSound);
            }

        }
        
    }

}
