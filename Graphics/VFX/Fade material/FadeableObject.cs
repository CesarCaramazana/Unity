using UnityEngine;
using System.Collections;


public class FadeObject : MonoBehaviour
{
    [SerializeField] private float lifetime = 3;
    [SerializeField] private float fadetime = 1;


    private IEnumerator Fade()
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