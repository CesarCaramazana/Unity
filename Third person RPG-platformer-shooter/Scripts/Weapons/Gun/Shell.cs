using UnityEngine;
using System.Collections;


public class Shell : MonoBehaviour
{

    public Rigidbody myRigidbody;
    [SerializeField] private float forceMin;
    [SerializeField] private float forceMax;

    [SerializeField] private float lifetime = 3;
    [SerializeField] private float fadetime = 1;

    void OnEnable()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColour = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColour, Color.clear, percent);
            yield return null;
        }

        gameObject.SetActive(false);
        mat.color = initialColour;
        yield return null;
    }
}