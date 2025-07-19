using UnityEngine;
using UnityEngine.UI;

public class GameProgressBar : MonoBehaviour
{
    public Slider verticalSlider;         // Drag your Slider here
    public Transform player;              // Drag your Player here
    public float minimumY = 0f;           // Bottom of level
    public float maximumY = 100f;         // Top of level (goal)

    void Update()
    {
        float currentY = player.position.y;
        float progress = Mathf.InverseLerp(minimumY, maximumY, currentY);
        verticalSlider.value = progress;
    }
}
