using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class GrappleHook : MonoBehaviour
{
    [Header("Grapple movement")]
    [SerializeField] private float pullSpeed = 15f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private float maxSpeed = 55f;
    private float speed;

    [Header("Grapple point detection")]
    [SerializeField] private float scanDistance = 60f;
    [SerializeField] private float maxAngle = 30f;

    [Space(5)]
    [SerializeField] private LayerMask grapplePointLayer;
    [SerializeField] private Transform grapplePoint;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private bool isGrappling;

    [Space(10)]
    [SerializeField] private Transform grappleGunHoldPoint;

    private Vector3 lastPullDirection;


    [SerializeField] private LineRenderer lineRenderer;

    private bool buttonPressed = false;



    private Transform mainCamera;
    private PlayerInput playerInput;

    private void Awake()
    {
        GetComponents();
    }

    // Start is called before the first frame update
    void Start()
    {        
        speed = pullSpeed;
    }


    private void GetComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main.transform;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement.Instance.isGrappling = isGrappling;

        if (isGrappling)
        {
            PullTowardsGrapplePoint();

            if (!TargetOnSight(grapplePoint) || TargetTooClose(grapplePoint)) ResetGrapple();
        }
        else Aim();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void PullTowardsGrapplePoint()
    {
        //Debug.Log("PULL");
        Vector3 pullDirection = (grapplePoint.position - transform.position).normalized;
        lastPullDirection = pullDirection;

        speed += acceleration * Time.deltaTime;
        speed = Mathf.Clamp(speed, pullSpeed, maxSpeed);

        Vector3 velocity = pullDirection * speed;

        Vector3 moveAmount = velocity * Time.deltaTime;

        PlayerMovement.Instance.playerController.Move(moveAmount);

    }

    private IEnumerator KeepMomentum()
    {
        //Debug.Log("Triggered coroutine KeepMomentum");
        float currentSpeed = speed;

        while(currentSpeed > 1f)
        {
            if (PlayerMovement.Instance.isGrounded || PlayerMovement.Instance.jumped) break;


            //Debug.Log("current speed * " + currentSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);

            Vector3 velocity = (lastPullDirection + 0.5f * transform.up) * currentSpeed;


            Vector3 moveAmount = velocity * Time.deltaTime;

            PlayerMovement.Instance.playerController.Move(moveAmount);

            yield return null;
        }
    }


    private void OnGrapple()
    {
        buttonPressed = !buttonPressed;

        //Holding down the button
        if (buttonPressed)
        {
            if (!isGrappling)
            {
                Scan();
            }
        }
        //Button release
        else if (!buttonPressed)
        {
            ResetGrapple();
        }
    }

    private void ResetGrapple()
    {
        isGrappling = false;
        grapplePoint = null;

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        StartCoroutine(KeepMomentum());

        speed = pullSpeed;
    }

    private bool TargetTooClose(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) < minDistance;
    }

    private bool TargetOnSight(Transform target)
    {
        return !Physics.Linecast(transform.position + Vector3.up * 0.5f, target.position, ~grapplePointLayer);
    }

    private void Scan()
    {
        // 1) Get all colliders in a sphere from player
        Collider[] nearbyTargets = Physics.OverlapSphere(transform.position, scanDistance, grapplePointLayer); // OverlapSphereNonAlloc?

        if (nearbyTargets.Length <= 0)
        {
            //Debug.Log("No targets in range");
            return; //No targets in range
        }
        else
        {
            //Get the target closest to the center of the screen (min angle between camera.forward and target)
            float closestAngle = maxAngle;
            Transform closestTarget = null;

            for (int i = 0; i < nearbyTargets.Length; i++)
            {
                Transform currentTarget = nearbyTargets[i].transform;

                //Debug.Log("\nTARGET: " + nearbyTargets[i].name);
                Vector3 cameraTargetDirection = currentTarget.position - mainCamera.position;
                //cameraTargetDirection.y = 0f; //Forget vertical angle

                float angle = Vector3.Angle(mainCamera.forward, cameraTargetDirection);

                bool onSight = TargetOnSight(currentTarget);

                if (onSight && (angle < closestAngle))
                {
                    closestAngle = angle;
                    closestTarget = currentTarget;
                }

                /*Debug.Log("Angle = " + angle);
                Debug.Log("Distance = " + distance);
                Debug.Log("On sight = " + onSight);*/
            }

            if (closestTarget == null)
            {
                isGrappling = false;
                grapplePoint = null;
                return;
            }
            else
            {
                isGrappling = true;
                grapplePoint = closestTarget;
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                //Debug.Log("Found point to grapple");
            }
        }
    }

    private void Aim()
    {
        grappleGunHoldPoint.transform.LookAt(mainCamera.forward * 200f);
    }

    private void DrawRope()
    {
        if (!grapplePoint)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        grappleGunHoldPoint.LookAt(grapplePoint.position);

        lineRenderer.SetPosition(0, grappleGunHoldPoint.position);
        lineRenderer.SetPosition(1, grapplePoint.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, scanDistance);

        if (grapplePoint != null) Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, grapplePoint.position);

    }



}
