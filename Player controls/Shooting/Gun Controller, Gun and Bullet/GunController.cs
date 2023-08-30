using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform gunHoldPoint;
    public Gun startingGun;

    [SerializeField] private Gun primaryGun;
    [SerializeField] private Gun secondaryGun;

    private bool primaryGunEquipped;

    private Gun equippedGun;

    // Start is called before the first frame update
    void Start()
    {
        if (startingGun != null)
        {
            primaryGun = startingGun;            
            primaryGunEquipped = true;
            EquipGun(startingGun);
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

    /*public void Shoot()
    {
        if (equippedGun != null)
        {
            equippedGun.Shoot();
        }
    }*/



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

    // Update is called once per frame
    void Update()
    {
        
    }
}
