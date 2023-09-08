using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeController : MonoBehaviour
{

    [SerializeField] private Transform swordHoldPoint;
    [SerializeField] private Sword startingSword;

    [SerializeField] private Sword primarySword;
    [SerializeField] private Sword secondarySword;


    private bool primarySwordEquipped;

    Sword equippedSword;



    //[SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {

        if (startingSword != null)
        {
            primarySword = startingSword;
            primarySwordEquipped = true;
            EquipSword(startingSword);
        }

    }


    public void EquipSword(Sword swordToEquip)
    {
        //If we already have a gun, destroy the currently equipped one
        if (equippedSword != null)
        {
            Destroy(equippedSword.gameObject);
        }

        equippedSword = Instantiate(swordToEquip, swordHoldPoint.position, swordHoldPoint.rotation) as Sword;
        equippedSword.transform.parent = swordHoldPoint;
    }



    public void MeleeAttack(MeleeAttackType attackType)
    {

        equippedSword.MeleeAttack(attackType);

    }




    public void SwitchSword()
    {

        if (primarySwordEquipped && secondarySword != null)
        {
            EquipSword(secondarySword);
            primarySwordEquipped = false;
        }
        else
        {
            EquipSword(primarySword);
            primarySwordEquipped = true;
        }


    }

}
