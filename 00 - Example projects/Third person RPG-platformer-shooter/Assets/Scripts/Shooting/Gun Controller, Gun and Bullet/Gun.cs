using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum FireMode { Auto, Burst, Single };


public class Gun : MonoBehaviour
{

    [SerializeField] private Transform muzzle;


    [Header("Fire mode")]
    [SerializeField] private FireMode fireMode;
    [SerializeField] private int burstCount = 1;
    [SerializeField] private int bulletsPerShot = 1;

    [Header("Bullet type")]
    [SerializeField] private ProjectileType bulletType;
    [SerializeField] private float muzzleSpeed; //Speed at which the bullet is fired (Gun dependent)

    [Header("Gun stats")]
    [SerializeField] private float timeBetweenShots; //Shooting rate (between bullets)
    [Range(0,1f)] [SerializeField] private float gunSpread; 
    [SerializeField] private float knockbackForce;


    //Player rigidbody
    private Rigidbody playerRigidbody;

    private bool triggerReleased = true;
    private int shotsRemainingInBurst;
    private float nextShotTime; //To keep track of the shooting rate


    [Header("Visual FX")]
    [SerializeField] private MuzzleFlash muzzleFlash;

    //SFX
    [Header("SFX")]
    [SerializeField] private AudioEventSO shootSFX;
    [SerializeField] private AudioEventSO reloadSFX;

    private void Start()
    {

        playerRigidbody = Player.Instance.GetComponent<Rigidbody>();

        //muzzleFlash = Instantiate(muzzleFlash, muzzle, false) as MuzzleFlash;

        shotsRemainingInBurst = burstCount;
        triggerReleased = true;
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

            // 1) Fire the bullet

            for (int i = 0; i < bulletsPerShot; i++)
            {
                Bullet bullet = SpawnBullet();
                bullet.SetBulletSpeed(muzzleSpeed);
                bullet.SetBulletDirection(gunSpread);

                SpawnCasquet();
            }               


            // 2) Update next shot time
            nextShotTime = Time.time + timeBetweenShots;

            ApplyKnockback();          

            if (shootSFX != null) shootSFX.Play();

            if (muzzleFlash != null) muzzleFlash.Activate(); 
        }
        
    }


    private void SpawnCasquet()
    {
        ObjectPool.Instance.SpawnPooledObject(ProjectileType.Casquet, muzzle.position, muzzle.rotation).GetComponent<Shell>();
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

        Vector3 recoilVelocity = -playerRigidbody.transform.forward * knockbackForce + playerRigidbody.transform.up * 0.5f;        
        playerRigidbody.AddForce(recoilVelocity, ForceMode.Impulse);

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

        //if (reloadSFX != null) reloadSFX.Play();
    }


}
