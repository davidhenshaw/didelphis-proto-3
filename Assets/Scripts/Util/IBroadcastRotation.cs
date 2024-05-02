using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBroadcastRotation 
{
    public delegate void BroadcastRotationDelegate(float oldRotation, float newRotation);
    public event BroadcastRotationDelegate Rotated;
}

public interface IRotationListener
{
    void OnRotationChanged(float oldRotation, float newRotation);
}
