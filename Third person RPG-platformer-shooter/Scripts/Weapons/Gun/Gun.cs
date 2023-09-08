using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum FireMode { Auto, Burst, Single, Charge };


public class Gun : MonoBehaviour
{

    [SerializeField] private Transform muzzle;


    [Header("Fire mode")]
    [SerializeField] private FireMode fireMode;

    [Header("Burst")]
    [SerializeField] private int burstCount = 1;
    [SerializeField] private int bulletsPerShot = 1;

    [Header("Charged")]
    [SerializeField] private float timeToCharge = 1f;
    [SerializeField] private bool isCharged;
    private float timeCharging = 0f;

    [Header("Bullet type")]
    [SerializeField] public ProjectileType bulletType;
    [SerializeField] private float muzzleSpeed = 20f; //Speed at which the bullet is fired (Gun dependent)
    [SerializeField] private float muzzleAcceleration = 0f; //If we want to accelerate the bullet 

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShots; //Shooting rate (between bullets)
    [Range(0,1f)] [SerializeField] private float gunSpread; 
    [SerializeField] private float knockbackForce;


    [Header("VFX")]
    [SerializeField] private float cameraShakeIntensity = 1f;
    [SerializeField] private MuzzleFlash muzzleFlash;


    //Player rigidbody
    //private Rigidbody playerRigidbody;

    private bool triggerReleased = true;
    private int shotsRemainingInBurst;
    private float nextShotTime; //To keep track of the shooting rate


    //SFX
    [Header("SFX")]
    [SerializeField] private AudioEventSO shootSFX;
    [SerializeField] private AudioEventSO onEquipSFX;
    [SerializeField] private AudioEventSO reloadSFX;
    [SerializeField] private AudioEventSO shellDropSFX;
    [SerializeField] private AudioEventSO chargingSFX;
    [SerializeField] private AudioEventSO chargedSFX;
    private bool chargedSFXplayed = false;
    private bool chargingSFXplayed = false;

    private void Start()
    {

        //playerRigidbody = Player.Instance.GetComponent<Rigidbody>();

        //muzzleFlash = Instantiate(muzzleFlash, muzzle, false) as MuzzleFlash;

        shotsRemainingInBurst = burstCount;
        triggerReleased = true;
    }

    private void OnEnable()
    {
        if (onEquipSFX != null) onEquipSFX.Play();
    }

    private void OnDisable()
    {
        //Debug.Log("Do i call disable on gun " + bulletType);
        //ObjectPool.Instance.ClearPool(bulletType);
    }

    public void Shoot()
    {
        
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleased)
                {
                    return;
                }
            }

            else if (fireMode == FireMode.Charge)
            {
                //Debug.Log("Time Charging = " + timeCharging + " | Is charged? " + isCharged + "|");
                isCharged = timeCharging >= timeToCharge;
                if(chargingSFX != null && !chargingSFXplayed) chargingSFX.Play(); chargingSFXplayed=true;
                if (!isCharged)
                {
                    timeCharging += Time.deltaTime;
                }
                else
                {
                    //Debug.Log("CHARGED");
                    if(chargedSFX != null && !chargedSFXplayed) chargedSFX.Play(); chargedSFXplayed = true;
                    if (chargingSFX != null) chargingSFX.Stop(); 
                }
                return;
            }

            // 1) Fire the bullet

            for (int i = 0; i < bulletsPerShot; i++)
            {
                ShootBullet();
            }               

            // 2) Update next shot time
            nextShotTime = Time.time + timeBetweenShots;

            //ApplyKnockback();        

            
        }
        
    }

    private void ShootBullet()
    {
        Bullet bullet = SpawnBullet();
        bullet.SetBulletSpeed(muzzleSpeed);
        bullet.SetBulletAcceleration(muzzleAcceleration);
        bullet.SetBulletDirection(gunSpread);
        SpawnCasquet();

        if (shootSFX != null) shootSFX.Play();
        if (muzzleFlash != null) muzzleFlash.Activate();


        CameraShake.Instance.Play(0.2f, cameraShakeIntensity);
    }


    private void SpawnCasquet()
    {
        ObjectPool.Instance.SpawnPooledObject(ProjectileType.Casquet, muzzle.position, muzzle.rotation).GetComponent<Shell>();
        if(shellDropSFX != null) shellDropSFX.Play();
    }


    private Bullet SpawnBullet()
    {
        /*Bullet bullet = ObjectPool.Instance.GetPooledObject().GetComponent<Bullet>();
        bullet.transform.localPosition = muzzle.position;
        bullet.transform.localRotation = muzzle.rotation;
        bullet.gameObject.SetActive(true);*/
        

        //Bullet bullet = ObjectPoolQueue.Instance.SpawnFromPool(bulletType, muzzle.position, muzzle.rotation).GetComponent<Bullet>();
        Bullet bullet = ObjectPool.Instance.SpawnPooledObject(bulletType, muzzle.position, muzzle.rotation).GetComponent<Bullet>();
        //Shell shellCasquet = ObjectPool.Instance.SpawnPooledObject(ProjectileType.Casquet, muzzle.position, muzzle.rotation).GetComponent<Shell>();

        return bullet;
    }




    private void ApplyKnockback()
    {       

        //Vector3 recoilVelocity = -playerRigidbody.transform.forward * knockbackForce + playerRigidbody.transform.up * 0.5f;        
        //playerRigidbody.AddForce(recoilVelocity, ForceMode.Impulse);

    }


    public void OnTriggerHold()
    {
        Shoot();
        triggerReleased = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleased = true;
        shotsRemainingInBurst = (shotsRemainingInBurst == 0) ? burstCount : shotsRemainingInBurst;

        if (fireMode == FireMode.Charge)
        {
            if (isCharged)
            {
                //Debug.Log("It is charged");
                //Debug.Log("SHOOT!");
                ShootBullet();
            }

            chargedSFXplayed = false;
            chargingSFXplayed = false;

            isCharged = false;
            timeCharging = 0f;
            
        }

        //if (reloadSFX != null) reloadSFX.Play();
    }


}
