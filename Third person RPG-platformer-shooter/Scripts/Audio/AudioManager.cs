using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;
    private static Dictionary<string, float> soundTimerDictionary;

    public static AudioManager instance;


    private void Awake()
    {


        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);


        soundTimerDictionary = new Dictionary<string, float>();

        //For every sound we need an AudioSource component: loop through the list, create the AS and assign the parameters stored in the Sound class
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;            
            sound.source.loop = sound.loop;

            if (sound.hasCooldown)
            {
                soundTimerDictionary[sound.name] = 0f;
            }
        }
    }



    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("Error: Sound " + name + " couldn't be found.");
            return;
        }

        if (!CanPlaySound(s)) return;


        s.source.PlayOneShot(s.source.clip);
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;               
            
        //Debug.Log("Playing sound  " + name);

    }


    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, s => s.name == name);

        if (s == null) return;

        s.source.Stop();
    }


    private bool CanPlaySound(Sound s)
    {
        if (soundTimerDictionary.ContainsKey(s.name))
        {
            float lastTimePlayed = soundTimerDictionary[s.name];

            if (lastTimePlayed + s.clip.length < Time.time)
            {
                soundTimerDictionary[s.name] = Time.time;
                return true;
            }

            return false;
        }
        return true;
    }

}
