using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{

    public GameObject spherePrefab;



    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            CreateOrbitingObject();
        }

    }



    private void CreateOrbitingObject()
    {
        Debug.Log("Instantiate object");
        Instantiate(spherePrefab, transform, false);
    }
}
