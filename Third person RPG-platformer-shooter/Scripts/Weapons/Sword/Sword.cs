using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;



public enum MeleeAttackType { Light, Heavy };



public class Sword : MonoBehaviour
{

    [SerializeField] private float lightAttackDamage;
    [SerializeField] private float heavyAttackDamage;

    private BoxCollider boxCollider;

    private float attackDamage;

    [SerializeField] private Animator animator;

    [Header("Combo system")]
    [SerializeField] private bool canAttack;
    
    private string triggerName;

    public string comboName = "";

    private static string lightAttack = "LightAttack";
    private static string heavyAttack = "HeavyAttack";


    [Header("SFX")]
    [SerializeField] private AudioEventSO lightAttackSFX;
    [SerializeField] private AudioEventSO heavyAttackSFX;


    [Header("Player")]
    [SerializeField] private Rigidbody playerRigidbody;


    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = Player.Instance.GetComponent<Rigidbody>();

        animator = Player.Instance.GetComponentInChildren<Animator>();  
        boxCollider = GetComponent<BoxCollider>();

        canAttack = true;

        attackDamage = 10f;

    }

    public void ResetCombo()
    {
        comboName = "";
    }




    private IEnumerator EnableDisableTransitions()
    {
        //animator.SetBool("CanTransition", true);
        yield return new WaitForSeconds(0.05f);
        //animator.SetBool("CanTransition", false);
    }    

    public void NextComboAttack()
    {
        //StopCoroutine(EnableDisableTransitions());
        StartCoroutine(EnableDisableTransitions());
    }

    public void MeleeAttack(MeleeAttackType attackType)
    {
        if (canAttack)
        {          
            //canAttack = false;

            if (attackType == MeleeAttackType.Light)
            {
                Debug.Log("Light attack in sword");
                triggerName = lightAttack;
            }

            if (attackType == MeleeAttackType.Heavy)
            {
                Debug.Log("HEavy attack in sword");
                triggerName = heavyAttack;
            }

            //Debug.Log(triggerName);
            animator.SetTrigger(triggerName);
            comboName = comboName + triggerName + "\n";

        }
    }



    public void PlayLightAttackSFX()
    {
        if (lightAttackSFX != null) lightAttackSFX.Play();

    }

    public void PlayHeavyAttackSFX()
    {
        if (heavyAttackSFX != null) heavyAttackSFX.Play();
    }



    public void ApplyForwardForce(float force)
    {
        Vector3 recoilVelocity = playerRigidbody.transform.forward * force;
        playerRigidbody.AddForce(recoilVelocity, ForceMode.Impulse);

    }

    public void ApplyKnockbackForce(float force)
    {
        Vector3 recoilVelocity = -playerRigidbody.transform.forward * force;
        playerRigidbody.AddForce(recoilVelocity, ForceMode.Impulse);

    }

    public void ApplyDownwardsForce(float force)
    {
        Vector3 recoilVelocity = -playerRigidbody.transform.up * force;
        playerRigidbody.AddForce(recoilVelocity, ForceMode.Impulse);

    }




    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Sword Trigger enter");

        IDamageable damageableObject = other.GetComponent<IDamageable>();

        if (damageableObject != null)
        {
            Debug.Log("Hit enemy");
            damageableObject.TakeHit(attackDamage);
        }

        if (other != null) ApplyKnockbackForce(50f);


    }



    //Hitbox
    public void EnableCollider()
    {
        boxCollider.enabled = true;
    }

    public void DisableCollider()
    {
        boxCollider.enabled = false;
    }
}
