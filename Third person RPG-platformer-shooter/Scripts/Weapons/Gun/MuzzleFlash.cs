using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{

    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private float flashTime = 0.03f;

    [SerializeField] private Sprite[] flashSprites;
    [SerializeField] private SpriteRenderer[] spriteRenderers;


    void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        muzzleFlash.SetActive(true);

        int flashSpriteIndex = Random.Range(0, flashSprites.Length);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }

        Invoke("Deactivate", flashTime);   
    }


    private void Deactivate()
    {
        muzzleFlash.SetActive(false);

    }
}
