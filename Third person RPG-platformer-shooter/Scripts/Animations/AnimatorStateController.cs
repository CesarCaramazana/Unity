using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateController : MonoBehaviour
{

    [SerializeField] Animator animator;
    //[SerializeField] PlayerController controller;


    //Hash variables (animator): optimization
    int isDashingHash;
    int speedHash;
    int isGroundedHash;
    int jumpedHash;
    int jumpCountHash;
    int onWallHash;
    int canJumpHash;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        isDashingHash = Animator.StringToHash("isDashing");
        speedHash = Animator.StringToHash("Speed");
        jumpedHash = Animator.StringToHash("Jumped");
        isGroundedHash = Animator.StringToHash("isGrounded");
        onWallHash = Animator.StringToHash("onWall");
        jumpCountHash = Animator.StringToHash("JumpCount");


    }

    // Update is called once per frame
    void Update()
    {
        SetAnimatorParameters();
        
    }

    private void SetAnimatorParameters()
    {
        if (animator != null)
        {

            animator.SetBool(isDashingHash, PlayerController.Instance.isDashing);
            animator.SetBool(isGroundedHash, PlayerController.Instance.isGrounded);
            animator.SetBool(onWallHash, PlayerController.Instance.onWall);
            //animator.SetFloat(speedHash, PlayerController.Instance.speed / PlayerController.Instance.maxSpeed);
            animator.SetFloat(speedHash, PlayerController.Instance.rb.velocity.magnitude / PlayerController.Instance.maxSpeed);
            animator.SetInteger(jumpCountHash, PlayerController.Instance.jumpCount);


            if (PlayerController.Instance.jumped && PlayerController.Instance.canJump) animator.SetTrigger(jumpedHash);


           // 
        }
    }
}
