using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class LockSystem : MonoBehaviour
{


    [Header("Lock")]
    [SerializeField] private bool lockedToEnemy = false;
    [SerializeField] private Transform enemyToLock;
    [SerializeField] private float rotateSpeed = 1f;


    [Header("Camera")]
    [SerializeField]
    private Transform mainCamera;

    [Space(20)]
    public InputManager input;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        LockToEnemy(); //Esto irá en un evento

        if (lockedToEnemy)
        {
            AimLockedToEnemy();
        }
        else
        {
            AimManual();
        }


        if (enemyToLock) Debug.DrawLine(transform.position, enemyToLock.position, Color.red);

    }


    private void AimManual()
    {
        //Get input direction 2D
        Vector2 inputDirection = input.GetAimingVectorNormalized();

        //Cast into Vector3
        Vector3 aimDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y);

        //Correct camera rotation
        aimDirection = Quaternion.Euler(0f, mainCamera.eulerAngles.y, 0f) * aimDirection;
        aimDirection = Vector3.Scale(aimDirection, new Vector3(1, 0, 1)).normalized;

        //Rotate the character so that it faces the direction it's moving (with Slerp to smooth it)
        transform.forward = Vector3.Slerp(transform.forward, aimDirection, rotateSpeed * Time.deltaTime);


    }

    private void AimLockedToEnemy()
    {
        if (enemyToLock != null)
        {
            Vector3 enemyPosition = enemyToLock.transform.position;
            Vector3 aimDirection = Vector3.Scale((enemyPosition - transform.position), new Vector3(1, 0, 1)).normalized;

            transform.forward = Vector3.Slerp(transform.forward, aimDirection, rotateSpeed * Time.deltaTime);
        }


    }

    private void LockToEnemy()
    {
        bool lockToEnemy = input.GetLockToEnemyPerformed();

        //If lock button pressed
        if (lockToEnemy)
        {
            Debug.Log("Lock button pressed");

            // If already locked to enemy -> undo lock
            if (lockedToEnemy)
            {
                Debug.Log("Already locked to enemy====> un-locking");
                enemyToLock = null;
                lockedToEnemy = false;
            }
            // If not locked to enemy: find the enemy at sight
            else
            {
                Debug.Log("Trying to find enemy");
                //Boxcast to find the enemy in the direction the player is facing
                //bool hitDetect = Physics.BoxCast(transform.position, 2* transform.localScale, transform.forward, out RaycastHit obj_hit);
                RaycastHit[] hitDetect = Physics.BoxCastAll(transform.position, 2 * transform.localScale, transform.forward);
                

                foreach (RaycastHit hit in hitDetect)
                {
                    //Vector3 hitPoint = hit.point;
                    //Debug.DrawLine(transform.position, hitPoint);

                    Debug.DrawLine(transform.position, hit.point);

                    Debug.Log("Hit with: " + hit.collider.gameObject.name);
                    if (hit.collider.gameObject.CompareTag("Enemy"))
                    {
                        enemyToLock = hit.collider.transform;
                        lockedToEnemy = true;
                        break;
                    }
                }

            }
        }

    }
}
