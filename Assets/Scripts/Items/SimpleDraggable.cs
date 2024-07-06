using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDraggable : MonoBehaviour, IDraggable
{
    public static int MAX_COLLIDER_DEPTH = 15;

    public delegate void DragEvent(Transform target);

    private Transform _followTarget;
    private Vector3 _offset;

    [SerializeField]
    private DraggableResponder[] _responders;

    public event IDraggable.DragEvent DragStarted;
    public event IDraggable.DragEvent DragFinished;
    public Action OnDragCallback;

    public GameObject Owner => gameObject;

    protected virtual void Start()
    {
        foreach(var responder in _responders)
        {
            DragStarted += responder.OnDragStart;
            DragFinished += responder.OnDragFinished;
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        if(_followTarget)
        {
            transform.position = _followTarget.position + _offset;
        }

    }

    protected virtual void OnSpringValue(float springValue)
    {
        //transform.eulerAngles = new Vector3(0, 0, springValue * _maxRotation * -1);
    }

    public virtual void OnDragStart(Transform target)
    {
        _followTarget = target;
        _offset = transform.position - target.position;
        DragStarted?.Invoke(_followTarget, _offset);
    }
    
    /// <summary>
    /// Invoked on every frame of the drag
    /// </summary>
    public virtual void OnDrag()
    {
        OnDragCallback?.Invoke();
        //float dX = transform.position.x - _prevXPosition;

        //_goalSpringPos = dX > 0 ? 1 : -1;
        //_goalSpringPos = Mathf.Abs(dX) < Mathf.Epsilon ? 0 : _goalSpringPos;

        //_prevXPosition = transform.position.x;
        //SpringUtil.CalcDampedSimpleHarmonicMotion(ref _springPosition, ref _springVelocity, _goalSpringPos, Time.deltaTime, _springFrequency, _springDamping);
        //OnSpringValue(_springPosition);
    }

    public virtual void OnDrop()
    {
        DragFinished?.Invoke(_followTarget, _offset);
        _followTarget = null;
        _offset = Vector2.zero;
        //transform.eulerAngles = Vector3.zero;
        //_springVelocity = 0;
    }
}
