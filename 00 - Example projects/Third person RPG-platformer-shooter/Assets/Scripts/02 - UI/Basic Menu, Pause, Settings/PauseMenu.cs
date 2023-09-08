using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //Imports the Input System package, instead of the Input Manage (by default)
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;


    // Update is called once per frame
    void Update()
    {
        //if (keyboard.escapeKey.IsPressed())

        //Old input system
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Debug.Log("Escape pressed");
            if (GameIsPaused){
                Resume();
            }
            else
            {
                Pause();
            }
        }        
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        GameIsPaused = false;

    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        GameIsPaused = true;   
    }

    public void OptionsMenu()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;
        GameIsPaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
