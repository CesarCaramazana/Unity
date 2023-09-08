using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class GameplayUI : MonoBehaviour
{
    [Header("Reference Game Objects")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Player player;
    [SerializeField] private Enemy enemy;


    [Header("Textbox")]
    //Score text
    [SerializeField] private TMP_Text scoreNumber;
    [SerializeField] private TMP_Text playerSpeed;
    [SerializeField] private TMP_Text remainingJumpsNumber;
    [SerializeField] private TMP_Text jumpCountNumber;
    [SerializeField] private TMP_Text comboText;

    [Header("Sliders")]
    //Player healthbar
    [SerializeField] private Slider playerHealthbarSlider;
    
    //Enemy healthbar
    [SerializeField] private Slider enemyHealthbarSlider;
    public Transform lockTarget;
    public bool isLockedOn = false;


    // Toggle
    [Header("Toggles")]
    
    [SerializeField] private Toggle onSlopeToggle;
    [SerializeField] private Toggle onAirToggle;
    [SerializeField] private Toggle onWallToggle;


    [SerializeField] private Toggle sprintingToggle;
    [SerializeField] private Toggle walkingToggle;
    [SerializeField] private Toggle dashingToggle;


    public static GameplayUI Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        playerHealthbarSlider.maxValue = Player.Instance.maxHealth;
        playerHealthbarSlider.value = Player.Instance.health;
        playerHealthbarSlider.minValue = 0.0f;

        enemyHealthbarSlider.gameObject.SetActive(false);
        InvokeRepeating("UpdateText", 0f, 0.5f);

    }

    private void SetEnemyHealthBar()
    {
        enemyHealthbarSlider.gameObject.SetActive(true);
        enemyHealthbarSlider.maxValue = lockTarget.GetComponent<Enemy>().maxHealth;
        enemyHealthbarSlider.value = lockTarget.GetComponent<Enemy>().health;
        enemyHealthbarSlider.minValue = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {

        //Player healthbar
        playerHealthbarSlider.value = Player.Instance.health;

        //Enemy healthbar
        if (isLockedOn) SetEnemyHealthBar();
        else enemyHealthbarSlider.gameObject.SetActive(false);


        //CHARACTER CONTROLLER
        onAirToggle.isOn = !PlayerMovement.Instance.isGrounded;
        onSlopeToggle.isOn = PlayerMovement.Instance.onSlope;
        onWallToggle.isOn = PlayerMovement.Instance.onWall;


        sprintingToggle.isOn = PlayerMovement.Instance.isSprinting;
        walkingToggle.isOn = PlayerMovement.Instance.isWalking;
        dashingToggle.isOn = PlayerMovement.Instance.isDashing;


        //Text
        remainingJumpsNumber.text = "Remaining jumps: " + PlayerMovement.Instance.remainingJumps.ToString();
        jumpCountNumber.text = "Jump count: " + PlayerMovement.Instance.jumpCount.ToString();


        UpdateText();
    }


    private void UpdateText()
    {

        //Score number (text)
        scoreNumber.text = gameManager.score.ToString();

        // Player controller
        //float velocity = PlayerController.Instance.speed;

        //float velocity = PlayerController.Instance.rb.velocity.magnitude;
        float velocity = PlayerMovement.Instance.playerController.velocity.magnitude;
        
        velocity = Mathf.Round(velocity * 10f) * 0.1f;


        //playerSpeed.text = "Velocity: " + PlayerController.Instance.rb.velocity.ToString() + "\n |" + velocity.ToString() +"|";

        playerSpeed.text = "Velocity: " + PlayerMovement.Instance.playerController.velocity.ToString() + "\n |" + velocity.ToString() +"|";
    }
}
