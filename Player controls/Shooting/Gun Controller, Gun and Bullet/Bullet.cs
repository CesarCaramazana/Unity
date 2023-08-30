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

    private IEnumerator deactivateBullet;

    private TrailRenderer trailRenderer;

    [SerializeField] LayerMask ignoreCollisionMask; //With which objects we want the bullet to collide


    [Header("Visual FX")]
    [SerializeField] private ParticleSystem hitFX;

    private float raycastBackOffset = 2.5f;

    [SerializeField] private Transform mainCamera;

    private Vector3 bulletDirection;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    private void OnEnable()
    {
        deactivateBullet = DeactivateBulletCoroutine(bulletTimeToLive);
        StartCoroutine(deactivateBullet);

    }


    

    private void Start()
    {
        //GameObject.Destroy(gameObject, bulletTimeToLive);
        trailRenderer = GetComponent<TrailRenderer>();
        mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        float moveDistance = bulletSpeed * Time.deltaTime;
        CheckBulletCollisions(moveDistance);
        //transform.Translate(Vector3.forward * moveDistance);
        transform.Translate(bulletDirection * moveDistance);
    }


    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }


    public void SetBulletDirection(float gunSpread)
    {
        Vector3 spreadDirection = new Vector3(Random.Range(-gunSpread, gunSpread), 0, Random.Range(-gunSpread, gunSpread));

        bulletDirection = Vector3.forward + spreadDirection;
        
    }




    private void CheckBulletCollisions(float moveDistance)
    {
        //Create a ray in the forward direction
        
        Ray ray = new Ray(transform.position - transform.forward * raycastBackOffset, transform.forward);

        RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, moveDistance, collisionMask))
        if (Physics.Raycast(ray, out hit, moveDistance + raycastBackOffset, ~ignoreCollisionMask))
        {
            OnHitObject(hit);

        }
    }
     
    private void OnHitObject(RaycastHit hit)
    {
        //Debug.Log("We hit: " + hit.collider.gameObject.name);

        Destroy(Instantiate(hitFX.gameObject, hit.point, Quaternion.FromToRotation(-Vector3.forward, hit.point)) as GameObject, 0.4f);


        //Hit the object and apply damage if it is damageable
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();

        if (damageableObject != null)
        {
            damageableObject.TakeHit(bulletDamage);
            
        }

        //Destroy the bullet
        //GameObject.Destroy(gameObject);
        
        
        // Trying object pooling --> does not work
        StopCoroutine(deactivateBullet);
        DeactivateBullet();
    }



    public IEnumerator DeactivateBulletCoroutine(float bulletTimeToLive)
    {
        yield return new WaitForSeconds(bulletTimeToLive);
        DeactivateBullet();
        //trailRenderer.enabled = false;
    }

    private void DeactivateBullet()
    {
        gameObject.SetActive(false);
        trailRenderer.Clear();

    }

}
