//Created by Devin Reimer (AlmostLogical Software) - http://www.almostlogical.com
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodySleepOnAwake : MonoBehaviour
{
    public void Awake()
    {
        GetComponent<Rigidbody>().Sleep();
    }
}