using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadRumble : MonoBehaviour
{
    public static GamepadRumble Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        
    }

    public void Play()
    {
        StopCoroutine(Rumble());
        StartCoroutine(Rumble());
    }

    public void Play(float rumbleDuration = 0.1f)
    {
        StopCoroutine(Rumble(rumbleDuration: rumbleDuration));
        StartCoroutine(Rumble(rumbleDuration: rumbleDuration));
    }

    public void Play(float lowFreq = 0.25f, float highFreq = 0.25f, float rumbleDuration = 0.1f)
    {
        StopCoroutine(Rumble(lowFreq, highFreq, rumbleDuration));
        StartCoroutine(Rumble(lowFreq, highFreq, rumbleDuration));
    }
    
    public void Play(float intensity = 0.25f, float rumbleDuration = 0.1f)
    {
        StopCoroutine(Rumble(intensity, intensity, rumbleDuration));
        StartCoroutine(Rumble(intensity, intensity, rumbleDuration));
    }

    public void Stop()
    {
        StopAllCoroutines();
    }


    private IEnumerator Rumble(float lowFreq = 0.25f, float highFreq = 0.25f, float rumbleDuration = 0.1f)
    {
        Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(rumbleDuration);
        Gamepad.current.SetMotorSpeeds(0, 0);
    }


}
