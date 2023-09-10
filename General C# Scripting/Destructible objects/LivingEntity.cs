using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float maxHealth;
    public float health;


    [SerializeField] AudioEventSO hitSFX;

    protected bool isAlive;

    // Start is called before the first frame update
    public virtual void Start()
    {
        health = maxHealth;
        isAlive = true;
    }

    public void TakeHit(float damage)
    {
        health -= damage;

        if(hitSFX != null) hitSFX.Play();

        if (health <= 0 && isAlive)
        {
            Die();
        }
    }


    protected void Die()
    {

        //Debug.Log("DEATH");
        isAlive = false;
        GameObject.Destroy(gameObject);

        if (TryGetComponent<DestructibleObject>(out DestructibleObject obj))
        {
            obj.Shatter();
        }
    }
}
