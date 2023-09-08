using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{

    [SerializeField]
    private Transform muzzle; //Point where the bullets spawn
    [SerializeField] private Bullet bulletPrefab;

    [SerializeField] private float timeBetweenShots = 0.1f; //Shooting rate
    [SerializeField] private float muzzleVelocity = 35f; //Speed at which the bullet is fired (Gun dependent)

    [SerializeField] private float recoil = 1f;
 

    private float nextShotTime; //To keep track of the shooting rate


    private void Start()
    {

    }


    

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            // 1) Fire the bullet
            //Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
            

            // With Object Pooling (to alleviate CPU cost) ---> Does not work
            Bullet bullet = ObjectPool.SharedInstance.GetPooledObject().GetComponent<Bullet>();
            bullet.transform.position = muzzle.position;
            bullet.transform.rotation = muzzle.rotation;
            bullet.gameObject.SetActive(true);

            /*if (bullet != null)
            {
                bullet.transform.position = muzzle.position;
                bullet.transform.rotation = muzzle.rotation;
                bullet.gameObject.SetActive(true);               

            }*/


            bullet.SetBulletSpeed(muzzleVelocity);


            // 2) Update next shot time
            nextShotTime = Time.time + timeBetweenShots;

        }



        
    }


}
