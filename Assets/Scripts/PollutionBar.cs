using UnityEngine;
using UnityEngine.UI;

public class PollutionBar : MonoBehaviour
{
    public Slider pollutionSlider;     // Assign your UI Slider
    public float pollutionMax = 5f;    // Player loses at 5
    public float animationSpeed = 2f;  // How fast the bar animates

    private float targetPollution = 0f;
    public GameObject gameOverPanel;
    void Start()
    {
        // Initialize the slider value
        pollutionSlider.value = 0f;
        targetPollution = 0f;
    }

    void Update()
    {
        // Smooth animation toward target
        if (pollutionSlider.value != targetPollution)
        {
            pollutionSlider.value = Mathf.MoveTowards(
                pollutionSlider.value,
                targetPollution,
                animationSpeed * Time.deltaTime
            );
        }
    }

    public void AddPollution(float amount)
    {
        targetPollution = Mathf.Clamp(targetPollution + (amount/ pollutionMax), 0f, pollutionMax);
        if(targetPollution >= 1f)
        {
            gameOverPanel.SetActive(true); // Show game over panel when pollution reaches max
        }
    }

    public void ResetPollution()
    {
        targetPollution = 0f;
    }
}
