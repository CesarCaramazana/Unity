using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public int score;


    private float startTime;


    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        score = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        score = (int) (Time.time - startTime);
        
    }


    public void GameOver()
    {
        Debug.Log("Game over");
    }


    public void Restart()
    {
        Debug.Log("Restart level");
        //score = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }




    
}
