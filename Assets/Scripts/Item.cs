using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IDraggable
{
    private Transform _followTarget;
    private Vector3 _offset;
 
    // Update is called once per frame
    void Update()
    {
        if(_followTarget)
        {
            transform.position = _followTarget.position + _offset;
        }
    }
    public void ClearTarget()
    {
        _followTarget = null;
        _offset = Vector2.zero;
    }

    public void SetDragTarget(Transform target)
    {
        _followTarget = target;
        _offset = transform.position - target.position;
    }
}
