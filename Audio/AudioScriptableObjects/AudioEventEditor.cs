using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioEventSO))]
public class AudioEventEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AudioEventSO audioEventScript = (AudioEventSO)target;

        if (GUILayout.Button("Preview"))
        {
            audioEventScript.PlayPreview();
        }
    }
}
