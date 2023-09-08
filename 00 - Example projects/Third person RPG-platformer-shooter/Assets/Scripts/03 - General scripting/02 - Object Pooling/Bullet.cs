using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float bulletSpeed = 10f;
    [SerializeField]
    public float bulletTimeToLive = 3f;
    [SerializeField]
    private float bulletDamage = 10f;

    public IEnumerator deactivateBullet;

    [SerializeField] LayerMask collisionMask; //With which objects we want the bullet to collide


    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable function");
        deactivateBullet = DeactivateBullet(bulletTimeToLive);
        StartCoroutine(deactivateBullet);

    }


    private void Start()
    {
        //GameObject.Destroy(gameObject, bulletTimeToLive);
    }

    private void Update()
    {
        float moveDistance = bulletSpeed * Time.deltaTime;
        CheckBulletCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    /*
    private void FixedUpdate()
    {
        float moveDistance = bulletSpeed * Time.fixedDeltaTime;
        CheckBulletCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }
    */



    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }


    private void CheckBulletCollisions(float moveDistance)
    {
        //Create a ray in the forward direction
        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, moveDistance, collisionMask))
        if (Physics.Raycast(ray, out hit, moveDistance))
        {
            OnHitObject(hit);

        }
    }

    private void OnHitObject(RaycastHit hit)
    {
        Debug.Log("We hit: " + hit.collider.gameObject.name);

        //Hit the object and apply damage if it is damageable
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();

        if (damageableObject != null)
        {
            damageableObject.TakeHit(bulletDamage, hit);
            
        }

        //Destroy the bullet
        //GameObject.Destroy(gameObject);
        
        
        // Trying object pooling --> does not work
        StopCoroutine(deactivateBullet);
        gameObject.SetActive(false); //Deactivate it if Object pooling
        //StopAllCoroutines();
        //StartCoroutine(DeactivateBullet(this, 0f));
    }



    public IEnumerator DeactivateBullet(float bulletTimeToLive)
    {
        Debug.Log("Deactivate bullet");
        yield return new WaitForSeconds(bulletTimeToLive);
        gameObject.SetActive(false);
    }
}
