using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : LivingEntity
{
    [SerializeField] private GameObject destroyedPrefab;

    public void Shatter()
    {
        GameObject shatteredObj = Instantiate(destroyedPrefab, transform.position, transform.rotation);
        //Destroy(gameObject);

        Destroy(shatteredObj, 5f);
    }

}
