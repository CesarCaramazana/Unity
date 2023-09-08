using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;


public enum SoundClipPlayOrder
{
    random,
    in_order,
    reverse
}

[CreateAssetMenu(fileName ="sfx_", menuName ="Audio/Audio Event", order =3)]
public class AudioEventSO : ScriptableObject
{

    public AudioClip[] clips;

    [Header("Config")]
    [SerializeField] private Vector2 volume = new Vector2(0.4f, 0.6f);
    [SerializeField] private Vector2 pitch = new Vector2(1, 1);
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private SoundClipPlayOrder playOrder;

    [SerializeField] private int playIndex = 0;

#if UNITY_EDITOR
    private AudioSource previewer;

    private void OnEnable()
    {
        previewer = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
    }


    private void OnDisable()
    {
        DestroyImmediate(previewer.gameObject);

    }

    public void PlayPreview()
    {
        Play(previewer);
    }


#endif


    private AudioClip GetAudioClip()
    {
        // get current clip
        var clip = clips[playIndex >= clips.Length ? 0 : playIndex];

        // find next clip
        switch (playOrder)
        {
            case SoundClipPlayOrder.in_order:
                playIndex = (playIndex + 1) % clips.Length;
                break;
            case SoundClipPlayOrder.random:
                playIndex = Random.Range(0, clips.Length);
                break;
            case SoundClipPlayOrder.reverse:
                playIndex = (playIndex + clips.Length - 1) % clips.Length;
                break;
        }

        // return clip
        return clip;
    }


    public AudioSource Play(AudioSource audioSourceParam = null)
    {
        if (clips.Length == 0)
        {
            Debug.LogError("Missing sound clips");
            return null;

        }


        var source = audioSourceParam;
        if (source == null)
        {
            var _obj = new GameObject("Sound", typeof(AudioSource));
            source = _obj.GetComponent<AudioSource>();
        }

        source.clip = GetAudioClip();
        source.volume = Random.Range(volume.x, volume.y);
        source.pitch = Random.Range(pitch.x, pitch.y);
        source.outputAudioMixerGroup = mixerGroup;

        source.Play();

#if UNITY_EDITOR
        if (source != previewer)
        {
            Destroy(source.gameObject, source.clip.length / source.pitch);
        }
//#else
            //Destroy(source.gameObject, source.clip.length / source.pitch);
#endif
        return source;



    }


}
