using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockableObject : MonoBehaviour
{

    [SerializeField] private Transform lockPoint;

    public Transform LockPoint()
    {
        return lockPoint;
    }

}
