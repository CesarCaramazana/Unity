using UnityEngine;
using System.Collections;


public class GeneralCoroutine : MonoBehaviour
{
    [SerializeField] private float completionTime = 1;

    private IEnumerator GeneralCoroutine()
    {
        float percent = 0;
        float speed = 1 / completionTime;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            //Here do an operation, like Linear interpolation between two values
            // mat.color = Color.Lerp(initialColour, Color.clear, percent);
            yield return null;
        }
    }
}