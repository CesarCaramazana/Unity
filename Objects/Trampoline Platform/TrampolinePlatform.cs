using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolinePlatform : MonoBehaviour
{

    [SerializeField] private float jumpSpeed = 40f;
    [SerializeField] private float deceleration = 1f;
    [SerializeField] private float cooldown = 1f;
    private float nextActivateTime;

    [Space(15)]
    [SerializeField] private Color originalColor;
    [SerializeField] private Color activatedColor;

    [Space(5)]
    [SerializeField] private float upMoveTime = 0.3f;
    [SerializeField] private float downMoveTime = 2f;
    [SerializeField] private float upOffset = 0.9f;

    Vector3 jumpDirection;

    [Space(10)]
    [SerializeField] private AudioEventSO sfx;
    [SerializeField] private ParticleSystem particlesVFX;

    private Vector3 originalPosition;
    private Vector3 upPosition;

    private Material mat;

    

    private void Start()
    {
        GetComponents();

        jumpDirection = transform.up;

        mat.color = originalColor;

        originalPosition = transform.position;
        upPosition = originalPosition + transform.up * upOffset;

        
        if (particlesVFX != null )
        {
            particlesVFX.Stop();

            ParticleSystem.MainModule main = particlesVFX.main;
            main.startColor = activatedColor;

            //particlesVFX.Play();

        }        
    }


    private void GetComponents()
    {
        mat = GetComponentInChildren<Renderer>().material;
        particlesVFX = GetComponentInChildren<ParticleSystem>();

        nextActivateTime = Time.time;

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time > nextActivateTime)
            {
                StopAllCoroutines();
                PlayerMovement.Instance.ApplyForce(jumpDirection, jumpSpeed, deceleration);
                StartCoroutine(PlatformSpringAnimation());

                nextActivateTime += cooldown;

                if (sfx) sfx.Play();
                if (particlesVFX != null) particlesVFX.Play();
            }           

        }            
    }


    private IEnumerator PlatformSpringAnimation()
    {  
        float upSpeed = 1 / upMoveTime;
        float downSpeed = 1 / downMoveTime;

        float percent = 0f;

        mat.color = activatedColor;

        while (percent < 1)
        {
            percent += Time.deltaTime * upSpeed;
            transform.position = Vector3.Lerp(transform.position, upPosition, percent);
            //mat.color = Color.Lerp(mat.color, activatedColor, percent);
            yield return null;
        }

        percent = 0f;

        while (percent < 1)
        {
            percent += Time.deltaTime * downSpeed;
            transform.position = Vector3.Lerp(transform.position, originalPosition, percent);
            mat.color = Color.Lerp(mat.color, originalColor, percent);
            yield return null;
        }
    }
    



}
