using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableResponder : MonoBehaviour, ISpringResponder, IRotationListener
{
    public float _maxRotation = 30;
    private Vector3 _rotationPoint;

    private Vector3 _origRotation;
    private void Awake()
    {
        _origRotation = transform.eulerAngles;

        //Find and listen to rotation broadcasts in parents
        foreach(IBroadcastRotation broadcast in GetComponentsInParent<IBroadcastRotation>())
        {
            broadcast.Rotated += OnRotationChanged;
        }
    }

    public void OnSpringValue(float springValue)
    {
        transform.eulerAngles = new Vector3(0, 0, _origRotation.z + springValue * _maxRotation * -1);
    }

    public void OnSpringRemoved()
    {
        transform.eulerAngles = _origRotation;
    }

    public void OnRotationChanged(float oldRotation, float newRotation)
    {
        _origRotation.z = newRotation;
    }
}
