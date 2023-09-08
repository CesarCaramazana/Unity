using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathCoroutine : MonoBehaviour
{

    public Vector3 destinationPoint = new Vector3(5,5, 5);
    public float moveSpeed = 3f;
    public float pauseTime = 1f;

    public Vector3[] path;

    IEnumerator currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        //currentCoroutine = MoveToPoint(destinationPoint, moveSpeed);
        //StartCoroutine(MoveInPath(path, moveSpeed));
    }

    private void Update()
    {
        //StopCoroutine(MoveToPoint(destinationPoint, moveSpeed));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("Spacebar pressed");
            if (currentCoroutine != null)
            {
                StopAllCoroutines(); //Since there's a MoveToPoint() coroutine inside MoveInPath(), we need to stop all.
            }

            currentCoroutine = MoveInPath(path, moveSpeed);
            StartCoroutine(currentCoroutine);
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            print("E key pressed");
            if (currentCoroutine != null)
            {
                StopAllCoroutines();
            }

            currentCoroutine = MoveToPointWithPause(destinationPoint, moveSpeed, pauseTime);
            StartCoroutine(currentCoroutine);

        }if (Input.GetKeyDown(KeyCode.A))
        {
            print("A key pressed");
            if (currentCoroutine != null)
            {
                StopAllCoroutines();
            }

            currentCoroutine = MoveToPoint(destinationPoint, moveSpeed);
            StartCoroutine(currentCoroutine);
        }

        
    }


    IEnumerator MoveToPoint(Vector3 point, float moveSpeed)
    {
        Debug.Log("Move to point");
        while (transform.position != point)
        {
            transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }


    IEnumerator MoveInPath(Vector3[] path, float moveSpeed)
    {
        foreach (Vector3 point in path)
        {
            Debug.Log("Point: " + point);
            yield return StartCoroutine(MoveToPoint(point, moveSpeed));
        } 
    }

    IEnumerator MoveToPointWithPause(Vector3 point, float moveSpeed, float pauseTime)
    {
        Debug.Log("Move with pause");
        while (transform.position != point)
        {
            transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(pauseTime);
        }
    }

}
