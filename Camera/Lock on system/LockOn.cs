using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
//using Cinemachine;



public class LockOn : MonoBehaviour
{

    [Header("Lock on")]
    [SerializeField] private float scanDistance = 60f;
    [SerializeField] private float maxScreenDistanceFactor = 0.5f;
    [SerializeField] private int maxColliders = 10;
    private float maxScreenDistance;

    [Space(5)]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private bool isLockedOn;

    [Header("Tolerance")]
    [SerializeField] private float keepLockDistanceOffset = 20f; //If already locked, keep the lock for a little bit more than the scan distance
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float keepOffSightTime = 2f; //How long the object must be blocked to lose lock on
    private float timeOffSight = 0f;

    [Space(10)]
    [SerializeField] private Transform lockTarget;

    [Header("Recenter")]
    [SerializeField] float recenterTime = 0.3f;


    [Header("Switch target")]
    [SerializeField] private float threshold = 0.6f;
    private float xInputValue;
    private bool hasTriggeredSwitchTarget;

    [Header("Reticle UI")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private SpriteRenderer reticle;

    [Header("Virtual cameras")]
    [SerializeField] private CinemachineFreeLook followCamera;
    [SerializeField] private CinemachineVirtualCamera lockCamera;

    private Transform mainCamera;
    private PlayerInput playerInput;

    private Vector2 screenCenter;

    // Start is called before the first frame update
    void Start()
    {
        GetComponents();

        reticle = Instantiate(reticle);        
        if(sprite != null) reticle.sprite = sprite;
        reticle.enabled = false;

        //lockCamera.transform.position = followCamera.transform.position;
        //lockCamera.transform.rotation = followCamera.transform.rotation;
        
    }    

    // Update is called once per frame
    void Update()
    {

        if (lockTarget)
        {
            DrawReticleOnTarget();
            TrySwitchTarget();


            if (!TargetOnSight(lockTarget))
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

        screenCenter.x = Screen.width / 2;
        screenCenter.y = Screen.height / 2;
        maxScreenDistance = Screen.width * maxScreenDistanceFactor;
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

    private void OnLook(InputValue inputValue)
    {
        xInputValue = inputValue.Get<Vector2>().x;
        //Debug.Log("Input direction in here " + xInputValue);

    }

    private void TrySwitchTarget()
    {
        if(xInputValue > threshold + 0.05f && !hasTriggeredSwitchTarget)
        {
            hasTriggeredSwitchTarget = true;
            SwitchTarget(+1);
            //Debug.Log("FIND NEW TARGET ON THE RIGHT");
        }
        else if (xInputValue < -threshold -0.05f && !hasTriggeredSwitchTarget)
        {
            hasTriggeredSwitchTarget = true;
            SwitchTarget(-1);
            //Debug.Log("FIND NEW TARGET ON THE LEFT");
        }

        else if ((xInputValue < threshold - 0.05f) && (xInputValue > -threshold + 0.05f) && hasTriggeredSwitchTarget)
        {
            //Debug.Log("Reset trigger");
            hasTriggeredSwitchTarget = false;
        }
    }



    private void DrawReticleOnTarget()
    {
        //Vector3 renderPosition;

        Vector3 direction = ((lockTarget.position) - mainCamera.position).normalized;

        if(Physics.Linecast(mainCamera.position, lockTarget.position, out RaycastHit hit, enemyLayer))
        {
            reticle.transform.position = hit.point - direction * 0.2f;
            reticle.transform.LookAt(mainCamera.position);
        }       

    }

    private IEnumerator RecenterFollowCamera(float recenterTime)
    {
        //Debug.Log("Recentering camera");

        followCamera.m_RecenterToTargetHeading.m_enabled = true;
        followCamera.m_YAxisRecentering.m_enabled = true;

        followCamera.m_YAxisRecentering.m_RecenteringTime = recenterTime / 2;
        followCamera.m_RecenterToTargetHeading.m_RecenteringTime = recenterTime/ 2;

        yield return new WaitForSeconds(recenterTime);

        followCamera.m_RecenterToTargetHeading.m_enabled = false;
        followCamera.m_YAxisRecentering.m_enabled = false;
        //Debug.Log("End recentering");
    }

    private void UnlockTarget()
    {
        if (isLockedOn)
        {
            //StopCoroutine(RecenterFollowCamera());
            //StartCoroutine(RecenterFollowCamera(0.05f));
            //RecenterFollowCamera();
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

        return ((distance < scanDistance + keepLockDistanceOffset) && (distance > minDistance));
    }

    private bool TargetOnSight(Transform target)
    {
        return !Physics.Linecast(transform.position + Vector3.up * 0.5f, target.position, ~enemyLayer);
        //return !Physics.Linecast(mainCamera.transform.position, target.position + Vector3.up * yOffset, ~enemyLayer); //This collides with the player
    }

    private void SwitchTarget(int polarity)
    {
        Collider[] hitColliders = new Collider[maxColliders];
        int numNearbyTargets = Physics.OverlapSphereNonAlloc(transform.position, scanDistance, hitColliders, enemyLayer);

        if (numNearbyTargets == 0)
        {
            return;
        }
        else
        {
            float closestDistance = Screen.width / 2;
            Transform closestTarget = null;

            for (int i = 0; i < numNearbyTargets; i++)
            {
                LockableObject lockableObject = hitColliders[i].GetComponent<LockableObject>();
                if (lockableObject == null)
                {
                    continue;
                }
                Transform currentTarget = lockableObject.LockPoint();
                Vector3 positionOnScreen = Camera.main.WorldToScreenPoint(currentTarget.position);

                //bool onSight = positionOnScreen.z > 0f;
                //bool onSight = TargetOnSight(currentTarget);
                bool onSight = (TargetOnSight(currentTarget) && (positionOnScreen.z > 0));
                if (!onSight) continue;


                float xDistance = (positionOnScreen.x - screenCenter.x) * polarity;
                if (xDistance < 0) continue;

                //Debug.Log("X difference = " + xDistance);

                if (xDistance < closestDistance && currentTarget != lockTarget)
                {
                    closestDistance = xDistance;
                    closestTarget = currentTarget;

                }
            }

            //Check if we have a valid target and enable Lock-On
            // No target
            
            if (closestTarget == null)
            {
                return;
            }
            // Target
            else
            {
                lockTarget = closestTarget;
                lockCamera.m_LookAt = lockTarget;
            }
            
        }

    }

    private void Scan()
    {
        
        Collider[] hitColliders = new Collider[maxColliders];

        int numNearbyTargets = Physics.OverlapSphereNonAlloc(transform.position, scanDistance, hitColliders, enemyLayer);

        //Debug.Log("Number of near targets NonAlloc = " +  numNearbyTargets);
        if (numNearbyTargets == 0)
        {
            StartCoroutine(RecenterFollowCamera(recenterTime));
            return;
        }
        else
        {
            float closestDistance = maxScreenDistance;
            Transform closestTarget = null;

            for (int i = 0; i < numNearbyTargets; i++)
            {
                //Debug.Log(hitColliders[i].gameObject.name);
                LockableObject lockableObject = hitColliders[i].GetComponent<LockableObject>();
                if (lockableObject == null)
                {
                    //Debug.Log("This one does not have a lock point");
                    continue;
                }

                Transform currentTarget = lockableObject.LockPoint();
                //Debug.Log("Current target = " +  currentTarget);


                Vector3 positionOnScreen = Camera.main.WorldToScreenPoint(currentTarget.position);

                //bool onSight = positionOnScreen.z > 0f;
                //bool onSight = TargetOnSight(currentTarget);
                bool onSight = (TargetOnSight(currentTarget) && (positionOnScreen.z > 0));
                if (!onSight) continue;

                //Debug.Log("Targtet on sight");


                float distance = Vector2.Distance(new Vector2(positionOnScreen.x, positionOnScreen.y), screenCenter);
                

                //Debug.Log("Position on the screen = " + positionOnScreen);
                //Debug.Log("Screen center = " + screenCenter);
                //Debug.Log("Distance to the center of the screen = " + distance);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = currentTarget;
                }
            }

            //Check if we have a valid target and enable Lock-On
            // No target
            if (closestTarget == null)
            {
                isLockedOn = false;
                lockTarget = null;
                StartCoroutine(RecenterFollowCamera(recenterTime));
                //RecenterFollowCamera();
                return;
            }
            // Target
            else
            {
                lockTarget = closestTarget;
                isLockedOn = true;
                reticle.enabled = true;

                lockCamera.m_LookAt = lockTarget;
                lockCamera.Priority = 11;
            }
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, scanDistance);

        if(lockTarget != null) Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, lockTarget.position);

    }
}
