using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingObject : MonoBehaviour,  IEquatable<FadingObject>
{


    public List<Renderer> Renderers = new List<Renderer>();
    public Vector3 Position;
    public List<Material> Materials = new List<Material>();

    [HideInInspector]
    public float initialOpacity;

    private void Awake()
    {
        Position = transform.position;

        if (Renderers.Count == 0)
        {
            Renderers.AddRange(GetComponentsInChildren<Renderer>());
        }

        foreach (Renderer renderer in Renderers)
        {
            Materials.AddRange(renderer.materials);
        }

        initialOpacity = 1;
    }


    public bool Equals(FadingObject other)
    {
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


}
