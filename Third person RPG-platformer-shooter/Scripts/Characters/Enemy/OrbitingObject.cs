using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrbitingObject : MonoBehaviour
{

    private Transform parentTransform;

    [Header("Object size")]
    [SerializeField] private float sphereRadius;
    [SerializeField] private Vector2 minMaxSphereRadius;
    [Header("Orbiting speed")]
    [SerializeField] private float orbitSpeed;
    [SerializeField] private Vector2 minMaxOrbitSpeed;
    [Header("Orbit radius")]
    [SerializeField] private float orbitRadius;
    [SerializeField] private Vector2 minMaxOrbitRadius;
    [Header("Orbit direction")]
    [SerializeField] private bool clockwiseRotation;

    private int clockwiseRotationFactor = 1;
    private float orbitChangeSpeed;


    private void SetRandomParameters()
    {
        //Sphere radius
        sphereRadius = Random.Range(minMaxSphereRadius.x, minMaxSphereRadius.y);
        //Orbiting speed
        orbitSpeed = Random.Range(minMaxOrbitSpeed.x, minMaxOrbitSpeed.y);
        //Orbiting radius
        orbitRadius = Random.Range(minMaxOrbitRadius.x, minMaxOrbitRadius.y) + sphereRadius;
        //Orbiting direction
        clockwiseRotation = Random.value > 0.5f;

    }   

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start of OO");

        SetRandomParameters();

        //Scale to sphere radius
        transform.localScale = Vector3.one * sphereRadius;

        //Set parent
        parentTransform = transform.parent;

        //Set orbit radius
        // Random intial position at Orbit Radius
        Vector2 randomInitialPosition = Random.insideUnitCircle.normalized * orbitRadius;
        // Offset vertically by sphereRadius/2 so that all spheres touch the ground at their base
        transform.localPosition = new Vector3(randomInitialPosition.x, sphereRadius / 2, randomInitialPosition.y);

        //Set orbit rotation direction
        clockwiseRotationFactor = clockwiseRotation ? 1 : -1;

    }

    // Update is called once per frame
    void Update()
    {
        OrbitAroundCenter();
    }


    private void OrbitAroundCenter()
    {
        //Debug.Log("Orbiting");
        Vector3 center = parentTransform.position;

        transform.RotateAround(center, Vector3.up, clockwiseRotationFactor * orbitSpeed * Time.deltaTime);
    }


    IEnumerator changeOrbitRadius(float radius)
    {
        Debug.Log("Change radius to " + radius);

        float currentRadius = (transform.position - parentTransform.position).magnitude;
        float radiusRatio = radius / currentRadius;

        Debug.Log("Current radius = " + currentRadius);

        int scaleFactor = 1;
        if (radius < currentRadius) scaleFactor = -1;

        while (Mathf.RoundToInt(currentRadius * 10) != Mathf.RoundToInt(radius * 10))
        {
            Vector3 radiusDirection = (transform.position - parentTransform.position).normalized;
            Vector3 newRadiusPosition = radiusDirection * radius;

            transform.position = transform.position + scaleFactor * radiusDirection * orbitChangeSpeed * Time.deltaTime;

            currentRadius = (transform.position - parentTransform.position).magnitude;
            print("Radius after update: " + currentRadius);
            yield return null;
        }
    }

}
