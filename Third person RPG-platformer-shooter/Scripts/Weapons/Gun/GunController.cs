using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    public Transform gunHoldPoint;
    public Gun startingGun;

    [SerializeField] private Gun primaryGun;
    [SerializeField] private Gun secondaryGun;

    [Header("Aiming")]
    [SerializeField] private float aimDistance = 1500f;
    [SerializeField] private LayerMask ignoreCollisionMask;


    private bool primaryGunEquipped;
    private Gun equippedGun;
    private Transform mainCamera;

    private float raycastOriginOffset = 10f;

    private bool isShooting;

    private PlayerInput playerInput;


    public static GunController Instance;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        GetComponents();

        if (startingGun != null)
        {
            primaryGun = startingGun;            
            primaryGunEquipped = true;
            EquipGun(startingGun);
        }

        Invoke("SetPlayerCameraDistance", 1f);
        
    }

    private void Update()
    {
        Aim();
        Shoot();
    }

    void GetComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main.transform;
    }


    private void SetPlayerCameraDistance()
    {
        raycastOriginOffset = (Player.Instance.transform.position - mainCamera.position).magnitude;
    }



    private void OnShoot(InputValue inputValue)
    {
        //Debug.Log("Shoot");
        isShooting = inputValue.isPressed;
        //Debug.Log(inputValue.isPressed);
    }


    private void Shoot()
    {
        if (isShooting)
        {
            OnTriggerHold();
        }
        else
        {
            OnTriggerRelease();
        }
    }


    private void OnSwitchGun()
    {
        SwitchGun();
    }


    private void Aim()
    {            
        //the "+mainCam.forward *offset is an offset so that we don't aim at objects between player and camera.
        if (Physics.Raycast(mainCamera.position + mainCamera.forward * raycastOriginOffset, mainCamera.forward, out RaycastHit hit, Mathf.Infinity, ~ignoreCollisionMask))
        {
            gunHoldPoint.transform.LookAt(hit.point);
        }

        else
        {
            gunHoldPoint.transform.LookAt(mainCamera.forward * aimDistance);
        }

    }


    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }


    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }

    }

    public void EquipGun(Gun gunToEquip)
    {
        //If we already have a gun, destroy the currently equipped one
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        equippedGun = Instantiate(gunToEquip, gunHoldPoint.position, gunHoldPoint.rotation) as Gun;
        equippedGun.transform.parent = gunHoldPoint;
    }



    public void SwitchGun()
    {
        //Debug.Log("Type of bullet from unequipped gun : " + equippedGun.bulletType);

        if (primaryGunEquipped && secondaryGun != null)
        {            
            EquipGun(secondaryGun);
            primaryGunEquipped = false;
        }
        else
        {
            EquipGun(primaryGun);
            primaryGunEquipped = true;
        }      

    }


}
