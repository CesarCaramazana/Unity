using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineBrain brain;

    [Space(10)]
    [SerializeField] private CinemachineFreeLook freelookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public static CameraShake Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        brain = GetComponent<CinemachineBrain>();        

    }


    public void Play(float duration, float intensity)
    {
        if((Object) brain.ActiveVirtualCamera == virtualCamera)
        {
            StartCoroutine(ShakeVirtualCamera(duration, intensity));
        }
        else if ((Object) brain.ActiveVirtualCamera == freelookCamera)
        {
            StartCoroutine(ShakeFreeLookCamera(duration, intensity));
        }
        else
        {
            Debug.LogError("No active camera matches the camera shake references");
        }            


    }


    private IEnumerator ShakeVirtualCamera(float duration, float intensity)
    {
        float time = Time.time;
        float endTime = time + duration;

        CinemachineBasicMultiChannelPerlin noiseProfile = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        noiseProfile.m_AmplitudeGain = intensity;

        while (time < endTime)
        {            
            time += Time.deltaTime;
            yield return null;
        }


        float fadeSpeed = 5f;
        while (noiseProfile.m_AmplitudeGain > 0.05f)
        {
            noiseProfile.m_AmplitudeGain = Mathf.Lerp(noiseProfile.m_AmplitudeGain, 0.0f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        noiseProfile.m_AmplitudeGain = 0.0f;
    }





    private IEnumerator ShakeFreeLookCamera(float duration, float intensity)
    {
        float time = Time.time;
        float endTime = time + duration;


        CinemachineBasicMultiChannelPerlin n1 = freelookCamera.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin n2 = freelookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin n3 = freelookCamera.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        n1.m_AmplitudeGain = intensity;
        n2.m_AmplitudeGain = intensity;
        n3.m_AmplitudeGain = intensity;

        while (time < endTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        float fadeSpeed = 5f;
        while (n1.m_AmplitudeGain > 0.05f && n2.m_AmplitudeGain > 0.05f && n3.m_AmplitudeGain > 0.05f)
        {
            n1.m_AmplitudeGain = Mathf.Lerp(n1.m_AmplitudeGain, 0.0f, Time.deltaTime * fadeSpeed);
            n2.m_AmplitudeGain = Mathf.Lerp(n2.m_AmplitudeGain, 0.0f, Time.deltaTime * fadeSpeed);
            n3.m_AmplitudeGain = Mathf.Lerp(n3.m_AmplitudeGain, 0.0f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        n1.m_AmplitudeGain = 0.0f;
        n2.m_AmplitudeGain = 0.0f;
        n3.m_AmplitudeGain = 0.0f;



    }

}
