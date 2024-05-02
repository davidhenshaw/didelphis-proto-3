using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationSpringController : MonoBehaviour
{

    [SerializeField]
    List<MonoBehaviour> xMotionResponders;

    [SerializeField]
    List<MonoBehaviour> yMotionResponders;
    private Vector3 _prevPosition;
    //[SerializeField]
    [Range(-1f, 1f)]
    private Vector2 _goalSpringPos = Vector2.zero;
    private Vector2 _springPosition = Vector2.zero;
    private Vector2 _springVelocity;

    [SerializeField]
    private float _springFrequency = 9f;
    [SerializeField]
    private float _springDamping = 0.5f;

    public Vector2 SpringPositions => _springPosition;

    public delegate void SpringValueUpdate(float springValue);
    public SpringValueUpdate xValueUpdated;
    public SpringValueUpdate yValueUpdated;
    public Action<Vector2> ValueUpdated;

    private void Awake()
    {
        _prevPosition = transform.position;

        foreach(ISpringResponder xResponder in xMotionResponders)
        {
            xValueUpdated += xResponder.OnSpringValue;
        }

        foreach(ISpringResponder yResponder in yMotionResponders)
        {
            yValueUpdated += yResponder.OnSpringValue;
        }
    }
    // Update is called once per frame
    void Update()
    {
        var deltaPos = transform.position - _prevPosition;
        _goalSpringPos.x = deltaPos.x > Mathf.Epsilon ? 1 : -1;
        _goalSpringPos.x = Mathf.Abs(deltaPos.x) < Mathf.Epsilon ? 0 : _goalSpringPos.x;

        _goalSpringPos.y = deltaPos.y > Mathf.Epsilon ? 1 : -1;
        _goalSpringPos.y = Mathf.Abs(deltaPos.y) < Mathf.Epsilon ? 0 : _goalSpringPos.y;

        var prevSpringPos = _springPosition;

        SpringUtil.CalcDampedSimpleHarmonicMotion(ref _springPosition.x, ref _springVelocity.x, _goalSpringPos.x, Time.deltaTime, _springFrequency, _springDamping);
        SpringUtil.CalcDampedSimpleHarmonicMotion(ref _springPosition.y, ref _springVelocity.y, _goalSpringPos.y, Time.deltaTime, _springFrequency, _springDamping);

        xValueUpdated?.Invoke(_springPosition.x);

        if (_springPosition.y != prevSpringPos.y)
        {
            yValueUpdated?.Invoke(_springPosition.y);
        }

        //if(_springPosition != prevSpringPos)
        //{
        //    ValueUpdated?.Invoke(_springPosition);
        //}
        _prevPosition = transform.position;
    }

}
