using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateController_CC : MonoBehaviour
{

    [SerializeField] Animator animator;
    //[SerializeField] PlayerController controller;


    //Hash variables (animator): optimization
    int isDashingHash;
    int speedHash;
    int verticalVelocityHash;
    int isGroundedHash;
    int jumpedHash;
    int jumpCountHash;
    int onWallHash;
    int canJumpHash;
    int onRightWallHash;
    int onLeftWallHash;
    int onFrontWallHash;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        isDashingHash = Animator.StringToHash("isDashing");
        speedHash = Animator.StringToHash("Speed");
        verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        jumpedHash = Animator.StringToHash("Jumped");
        isGroundedHash = Animator.StringToHash("isGrounded");
        jumpCountHash = Animator.StringToHash("JumpCount");
        onWallHash = Animator.StringToHash("onWall");
        onFrontWallHash = Animator.StringToHash("onFrontWall");
        onRightWallHash = Animator.StringToHash("onRightWall");
        onLeftWallHash = Animator.StringToHash("onLeftWall");


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

            animator.SetBool(isDashingHash, PlayerMovement.Instance.isDashing);
            animator.SetBool(isGroundedHash, PlayerMovement.Instance.isGrounded);
            animator.SetBool(onWallHash, PlayerMovement.Instance.onWall);
            animator.SetFloat(speedHash, PlayerMovement.Instance.speed / PlayerMovement.Instance.maxSpeed);
            animator.SetFloat(verticalVelocityHash, PlayerMovement.Instance.verticalVelocity);

            animator.SetBool(onFrontWallHash, PlayerMovement.Instance.wallOnFront);
            animator.SetBool(onLeftWallHash, PlayerMovement.Instance.wallOnLeft);
            animator.SetBool(onRightWallHash, PlayerMovement.Instance.wallOnRight);

            if (PlayerMovement.Instance.jumped && PlayerMovement.Instance.canJump)
            {
                //*!!!!!!!!! The order here matters because it will trigger the 2nd jump instead if we update the jump count before 
                animator.SetTrigger(jumpedHash);
                animator.SetInteger(jumpCountHash, PlayerMovement.Instance.jumpCount);

            } 

        }
    }


}
