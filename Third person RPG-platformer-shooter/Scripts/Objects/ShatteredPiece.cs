using System.Collections;
using UnityEngine;

public class ShatteredPiece : MonoBehaviour
{

    private Rigidbody rb;

    [SerializeField] private Vector2 minmaxExplosionForce = new Vector2(500f, 1000f);
    [SerializeField] private Vector2 minmaxExplosionRadius = new Vector2(10f, 100f);

    [SerializeField] private float lifetime = 1;
    [SerializeField] private float fadetime = 3;


    private void OnEnable()
    {

        float force = Random.Range(minmaxExplosionForce.x, minmaxExplosionForce.y); 
        float radius = Random.Range(minmaxExplosionRadius.x, minmaxExplosionRadius.y); 

        rb = GetComponent<Rigidbody>();
        rb.AddExplosionForce(force, transform.position, radius);

        StartCoroutine(Fade());
        
    }



    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        fadetime = fadetime + Random.Range(0f, 2f);

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

        //gameObject.SetActive(false);
        Destroy(gameObject);
    }


}
