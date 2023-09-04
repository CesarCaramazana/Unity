using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrailVFX : MonoBehaviour
{
    [SerializeField] public float trailDuration = 1.0f;

    private bool isTrailActive;

    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float meshRefreshRate = 0.1f;

    [SerializeField] private Transform positionToSpawn;

    [SerializeField] private Material trailMaterial;

    [SerializeField] SkinnedMeshRenderer[] skinnedMeshRenderers;


    // Start is called before the first frame update
    void Start()
    {
        positionToSpawn = transform.parent;
        //skinnedMeshRenderers = GetComponentsInParent<SkinnedMeshRenderer>();
        skinnedMeshRenderers = transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>();
        //skinnedMeshRenderers = GetComponents<SkinnedMeshRenderer>();



    }



    public void Play()
    {
        if (!isTrailActive)
        {
            StartCoroutine(ActivateTrail(trailDuration));
        }
    }


    private IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            isTrailActive = true;
            timeActive -= meshRefreshRate;

            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject obj = new GameObject();
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = trailMaterial;

                StartCoroutine(FadeMaterial(mr.material));

                Destroy(obj, lifetime);

            }


            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
            

    }



    private IEnumerator FadeMaterial(Material material)
    {

        float fadeSpeed = 3f;
        float fadeTime = lifetime;

        float time = Time.time;

        while (Time.time < fadeTime + time)
        {
            float alpha = Mathf.Lerp(material.GetFloat("_Alpha"), 0f, Time.deltaTime * fadeSpeed);
            material.SetFloat("_Alpha", alpha);
            yield return null;
        }


    }
}
