using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Collider2D))]
public class SimpleDraggable : MonoBehaviour, IDraggable
{
    private Transform _followTarget;
    private Vector3 _offset;

    private float _prevXPosition = 0;
    private float _maxRotation = 30;
    private float _rotationPerUnit = 0.7f;
    private float _springPosition = 0;
    private float _springVelocity;
 
    // Update is called once per frame
    protected void Update()
    {
        if(_followTarget)
        {
            transform.position = _followTarget.position + _offset;
        }

        float dX = transform.position.x - _prevXPosition;
        float goalRotation = Mathf.Clamp(dX * _rotationPerUnit, _maxRotation * -1, _maxRotation);

        SpringUtil.CalcDampedSimpleHarmonicMotion(ref _springPosition, ref _springVelocity, goalRotation, Time.deltaTime, 2, 0.75f);
        OnSpringValue(_springPosition);
        _prevXPosition = transform.position.x;
    }

    protected virtual void OnSpringValue(float springValue)
    {
        transform.eulerAngles = new Vector3(0, 0, springValue);
    }

    public virtual void OnDragStart()
    {
        
    }
    
    /// <summary>
    /// Invoked on every frame of the drag
    /// </summary>
    public virtual void OnDrag()
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
