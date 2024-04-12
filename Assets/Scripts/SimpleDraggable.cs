using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Collider2D))]
public class SimpleDraggable : MonoBehaviour, IDraggable
{
    private Transform _followTarget;
    private Vector3 _offset;
 
    // Update is called once per frame
    protected void Update()
    {
        if(_followTarget)
        {
            transform.position = _followTarget.position + _offset;
        }
    }

    public virtual void OnDragStart()
    {
        
    }

    public virtual void OnDrop()
    {
        _followTarget = null;
        _offset = Vector2.zero;
    }

    public virtual void SetDragTarget(Transform target)
    {
        _followTarget = target;
        _offset = transform.position - target.position;
    }
}
