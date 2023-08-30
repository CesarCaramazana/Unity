using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObjectBlockingObject : MonoBehaviour
{

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform target;
    [SerializeField]
    [Range(0, 1f)] private float fadedOpacity = 0.4f;
    //[SerializeField] private bool retainShadows = true;
    [SerializeField] private Vector3 targetPositionOffset = Vector3.up;
    [SerializeField] private float fadeSpeed = 1f;


    [Header("Read Only Data")]
    [SerializeField] List<FadingObject> objectsBlockingView = new List<FadingObject>();
    private Dictionary<FadingObject, Coroutine> runningCoroutines = new Dictionary<FadingObject, Coroutine>();

    private RaycastHit[] hits = new RaycastHit[5];

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(CheckForObjects());
        
    }


    private IEnumerator CheckForObjects()
    {
        //Debug.Log("Init coroutine");
        while (true)
        {
            int rayhits = Physics.RaycastNonAlloc(
                mainCamera.transform.position,
                (target.transform.position + targetPositionOffset - mainCamera.transform.position).normalized,
                hits,
                Vector3.Distance(mainCamera.transform.position, target.transform.position + targetPositionOffset),
                layerMask
                );


            if (rayhits > 0)
            {
                //Debug.Log("Hit objects");
                for (int i = 0; i < rayhits; i++)
                {
                    FadingObject fadingObject = GetFadingObjectFromHit(hits[i]);

                    if ( fadingObject != null && !objectsBlockingView.Contains(fadingObject))
                    {
                        if (runningCoroutines.ContainsKey(fadingObject))
                        {
                            if (runningCoroutines[fadingObject] != null)
                            {
                                StopCoroutine(runningCoroutines[fadingObject]);
                            }

                            runningCoroutines.Remove(fadingObject);
                        }

                        runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
                        objectsBlockingView.Add(fadingObject);
                    }
                }

            }

            FadeObjectsNoLongerBeingHit();

            ClearHits();

            yield return null;

        }

        
    }


    private IEnumerator FadeObjectOut(FadingObject fadingObject)
    {

        //Debug.Log("FADE OUT");

        float time = 0;
        float opacity = fadingObject.Materials[0].GetFloat("_Opacity");

        //while (fadingObject.Materials[0].color.a > fadedOpacity)
        while (fadingObject.Materials[0].GetFloat("_Opacity") > fadedOpacity)
        {
            foreach(Material material in fadingObject.Materials)
            {
                if (material.HasProperty("_Opacity"))
                {
                    opacity = Mathf.Lerp(opacity, fadedOpacity, time * fadeSpeed);
                    //material.color = new Color(material.color.r, material.color.g, material.color.b, Mathf.Lerp(fadingObject.InitialAlpha, fadedOpacity, time * fadeSpeed));
                    material.SetFloat("_Opacity", opacity);
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (runningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(runningCoroutines[fadingObject]);
            runningCoroutines.Remove(fadingObject);

        }

    }
    
    private IEnumerator FadeObjectIn(FadingObject fadingObject)
    {
        //Debug.Log("FADE INT");

        float time = 0;
        float opacity = fadingObject.Materials[0].GetFloat("_Opacity");
        while (fadingObject.Materials[0].GetFloat("_Opacity") < fadingObject.initialOpacity)
        {
            foreach(Material material in fadingObject.Materials)
            {
                if (material.HasProperty("_Opacity"))
                {
                    opacity = Mathf.Lerp(opacity, fadingObject.initialOpacity, time * fadeSpeed);
                    material.SetFloat("_Opacity", opacity);
                    //material.color = new Color(material.color.r, material.color.g, material.color.b, Mathf.Lerp(fadedOpacity, fadingObject.InitialAlpha, time * fadeSpeed));   
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (runningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(runningCoroutines[fadingObject]);
            runningCoroutines.Remove(fadingObject);

        }

    }



    private void FadeObjectsNoLongerBeingHit()
    {
        List<FadingObject> objectsToRemove = new List<FadingObject>(objectsBlockingView.Count);


        foreach (FadingObject fadingObject in objectsBlockingView)
        {
            bool objectIsBeingHit = false;

            for (int i = 0; i < hits.Length; i++)
            {
                FadingObject hitFadingObject = GetFadingObjectFromHit(hits[i]);

                if (hitFadingObject != null && fadingObject == hitFadingObject)
                {
                    objectIsBeingHit = true;
                    break;
                }
            }


            if (!objectIsBeingHit)
            {
                if (runningCoroutines.ContainsKey(fadingObject))
                {
                    if (runningCoroutines[fadingObject] != null)
                    {
                        StopCoroutine(runningCoroutines[fadingObject]);
                    }
                    runningCoroutines.Remove(fadingObject); 
                }

                runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectIn(fadingObject)));
                objectsToRemove.Add(fadingObject);
            }
        }

        foreach(FadingObject removeObject in objectsToRemove)
        {
            objectsBlockingView.Remove(removeObject);
        }
    }



    

    private void ClearHits()
    {
        System.Array.Clear(hits, 0, hits.Length);
    }


    private FadingObject GetFadingObjectFromHit(RaycastHit hit)
    {
        return hit.collider != null ? hit.collider.GetComponent<FadingObject>() : null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
