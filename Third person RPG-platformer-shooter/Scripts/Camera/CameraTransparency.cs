using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraTransparency : MonoBehaviour
{

    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;

    [SerializeField] private float playerCameraDistance;

    [SerializeField] private LayerMask playerMask;


    [SerializeField] private Material material;

    private RaycastHit[] hits;
    private List<GameObject> activeGameObjects = new List<GameObject>();

    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        playerCameraDistance = (mainCamera.position - player.position).magnitude;
        
    }

    // Update is called once per frame
    void Update()
    {

        RaycastCameraPlayer();        
       
        
    }


    private void RaycastCameraPlayer()
    {
        List<GameObject> hitGameObjects = new List<GameObject>();

        hits = Physics.RaycastAll(mainCamera.position, player.position - mainCamera.position, playerCameraDistance,~playerMask);

        //Get objects that are hit by the raycast and save them in a List
        foreach (RaycastHit hit in hits)
        {            

            if (activeGameObjects.Contains(hit.collider.gameObject))
            {
                //Debug.Log("We already have this game object");
            }
            else
            {
                activeGameObjects.Add(hit.collider.gameObject);
                MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
                material = renderer.material;

                EnsableDitheringTransparency();
            }

            hitGameObjects.Add(hit.collider.gameObject);
            
        }

        //Debug.Log("HIT: " + hitGameObjects);
        //Debug.Log("ACTIVE: " + activeGameObjects);

        //Get the objects that are active but not hit in this frame
        List<GameObject> listDifference = activeGameObjects.Except(hitGameObjects).ToList();

        //Debug.Log("DIFF: " + listDifference);

        foreach (GameObject gameObject in listDifference)
        {
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            material = renderer.material;
            DisableDitheringTransparency();



        }
        activeGameObjects = hitGameObjects;

        //Debug.Log("ACTIVE AFTER REMOVE: " + activeGameObjects);



    }




    private void EnsableDitheringTransparency()
    {
        print("Set dithering");
        material.SetFloat("_Opacity", 0.5f);

    }


    private void DisableDitheringTransparency()
    {
        material.SetFloat("_Opacity", 1f);

    }

}
