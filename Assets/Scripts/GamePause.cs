using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePause : MonoBehaviour
{

    private bool isPaused = false;
    public GameObject pauseMenuUI;
    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // Stop all movement and physics
        isPaused = true;
        if (pauseMenuUI)
        {
            pauseMenuUI.SetActive(true); // Show the pause menu UI
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume normal speed
        isPaused = false; 
        pauseMenuUI.SetActive(false);
    }


}
