using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
//using Cinemachine;


public enum LockOnCriteria { Angle, Distance };

public class LockOn : MonoBehaviour
{

    

    [Header("Lock on")]
    [SerializeField] private float scanDistance = 60f;
    [SerializeField] private float maxLockAngle = 60f;
    [SerializeField] private LockOnCriteria criteria = LockOnCriteria.Angle;

    [Space(5)]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private bool isLockedOn;

    [Header("Tolerance")]
    [SerializeField] private float keepLockDistanceOffset = 20f; //If already locked, keep the lock for a little bit more than the scan distance
    [SerializeField] private float keepOffSightTime = 2f; //How long the object must be blocked to lose lock on
    [SerializeField] private float timeOffSight = 0f;

    [Space(10)]
    [SerializeField] private Transform lockTarget;
    [SerializeField] private float lockYOffset;

    [Header("Reticle UI")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private SpriteRenderer reticle;

    [Header("Virtual cameras")]
    [SerializeField] private CinemachineFreeLook followCamera;
    [SerializeField] private CinemachineVirtualCamera lockCamera;

    private Transform mainCamera;
    private PlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        GetComponents();

        reticle = Instantiate(reticle);        
        if(sprite != null) reticle.sprite = sprite;
        reticle.enabled = false;

        //lockCamera.transform.position = followCamera.transform.position;
        lockCamera.transform.rotation = followCamera.transform.rotation;
        
    }    

    // Update is called once per frame
    void Update()
    {

        if (lockTarget)
        {
            DrawReticleOnTarget();

            if (!TargetOnSight(lockTarget, lockYOffset))
            {
                timeOffSight += Time.deltaTime;
            }
            else timeOffSight = 0f;

            if (!TargetOnRange() || timeOffSight > keepOffSightTime) UnlockTarget();

        }
        else UnlockTarget();

        PlayerMovement.Instance.isLockedOn = isLockedOn;
        PlayerMovement.Instance.lockTarget = lockTarget;
        GameplayUI.Instance.isLockedOn = isLockedOn;
        GameplayUI.Instance.lockTarget = lockTarget;

    }
    


    private void GetComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main.transform;

    }





    private void OnLockOn()
    {
        //Debug.Log("Player input Lock On action");

        if(lockTarget != null)
        {
            UnlockTarget();
        }
        else
        {
            Scan();
        }
    }


    private void DrawReticleOnTarget()
    {
        //Vector3 renderPosition;

        Vector3 direction = ((lockTarget.position + Vector3.up * lockYOffset) - mainCamera.position).normalized;

        if(Physics.Linecast(mainCamera.position, lockTarget.position + Vector3.up * lockYOffset, out RaycastHit hit, enemyLayer))
        {
            reticle.transform.position = hit.point - direction * 0.2f;
            reticle.transform.LookAt(mainCamera.position);
        }
        

    }

    private IEnumerator RecenterFollowCamera()
    {
        float endTime = Time.time + 2 * Time.deltaTime;
        //Debug.Log("Recentering camera");

        followCamera.m_RecenterToTargetHeading.m_enabled = true;
        followCamera.m_YAxisRecentering.m_enabled = true;
        while (Time.time < endTime)
        {
            yield return null;
        }
        followCamera.m_RecenterToTargetHeading.m_enabled = false;
        followCamera.m_YAxisRecentering.m_enabled = false;
        //Debug.Log("End recentering");
    }

    private void UnlockTarget()
    {
        if (isLockedOn)
        {
            StopCoroutine(RecenterFollowCamera());
            StartCoroutine(RecenterFollowCamera());
        }

        isLockedOn = false;
        lockTarget = null;
        reticle.enabled = false;


        //followCamera.Priority = 10;
        lockCamera.Priority = 9;
    }


    private bool TargetOnRange()
    {
        float distance = (transform.position - lockTarget.position).magnitude;

        return (distance < scanDistance + keepLockDistanceOffset);
    }

    private bool TargetOnSight(Transform target, float yOffset)
    {
        return !Physics.Linecast(transform.position + Vector3.up * 0.5f, target.position + Vector3.up * yOffset, ~enemyLayer);
    }

    private void Scan()
    {
        // 1) Get all colliders in a sphere from player
        Collider[] nearbyTargets = Physics.OverlapSphere(transform.position, scanDistance, enemyLayer); // OverlapSphereNonAlloc?

        if (nearbyTargets.Length <= 0)
        {
            //Debug.Log("No targets in range");
            return; //No targets in range
        }
        else
        {
            //Get the target closest to the center of the screen (min angle between camera.forward and target)
            float closestAngle = maxLockAngle;
            float closestDistance = scanDistance;
            Transform closestTarget = null;

            for (int i = 0; i < nearbyTargets.Length; i++)
            {
                Transform currentTarget = nearbyTargets[i].transform;

                //Debug.Log("\nTARGET: " + nearbyTargets[i].name);
                Vector3 cameraTargetDirection = currentTarget.position - mainCamera.position;
                cameraTargetDirection.y = 0f; //Forget vertical angle

                float angle = Vector3.Angle(mainCamera.forward, cameraTargetDirection);
                float distance = (currentTarget.position - transform.position).magnitude;

                bool onSight = TargetOnSight(currentTarget, 0.5f);
                //bool onSight = true;

                if (criteria == LockOnCriteria.Angle)
                {
                    if (angle < closestAngle && onSight)
                    {
                        closestAngle = angle;
                        closestTarget = currentTarget;
                    }

                }
                else if (criteria == LockOnCriteria.Distance && onSight)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = currentTarget;
                    }
                }


                /*Debug.Log("Angle = " + angle);
                Debug.Log("Distance = " + distance);
                Debug.Log("On sight = " + onSight);*/
            }

            if (closestTarget == null)
            {
                isLockedOn = false;
                lockTarget = null;
                return;
            }
            else
            {
                //Debug.Log("The closest target is " + closestTarget);

                CapsuleCollider targetCollider = closestTarget.GetComponent<CapsuleCollider>();
                lockYOffset = (targetCollider.height * closestTarget.localScale.y) / 2;

                /*Debug.Log("Height collider = " + targetCollider.height);
                Debug.Log("Scale = " + closestTarget.localScale.y);
                Debug.Log("Yoffset " + lockYOffset);*/

                lockTarget = closestTarget;
                isLockedOn = true;
                reticle.enabled = true;

                lockCamera.m_LookAt = lockTarget;
                lockCamera.Priority = 11;
                lockCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = lockYOffset;

            }
        }                
    }



    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, scanDistance);

        if(lockTarget != null) Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, lockTarget.position + Vector3.up * lockYOffset);

    }
}
