using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerInput playerInput;
    
    public static bool gamePaused = false;


    private void Start()
    {
        GetComponents();
        
    }

    private void GetComponents()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnPause()
    {
        //Debug.Log("Paused the game");

        if (gamePaused)
        {            
            Resume();            
        }
        else
        {            
            Pause();
        }
    }


    public void Pause()
    {
        playerInput.SwitchCurrentActionMap("MenuControls");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;

        gamePaused = true;
    }

    public void Resume()
    {
        playerInput.SwitchCurrentActionMap("Player");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;        

        gamePaused = false;
    }


    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }


    public void Restart()
    {
        Time.timeScale = 1.0f;
        gamePaused = false;

        gameManager.Restart();
    }
}
