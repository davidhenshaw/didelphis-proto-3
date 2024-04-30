using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSquash : MonoBehaviour
{

    public float VerticalSquishAmount = 0.5f;
    public float DefaultVerticalScale = 1;
    private Vector3 _prevPosition;
    //[SerializeField]
    [Range(-1f, 1f)]
    private float _goalSpringPos = 0;
    private float _springPosition = 0;
    private float _springVelocity;

    [SerializeField]
    private float _springFrequency = 0.4f;
    [SerializeField]
    private float _springDamping = 0.5f;

    public float XSpringPos => _springPosition;

    public delegate void OnSpringValue(float springValue);
    private List<OnSpringValue> _listeners = new();

    private void Awake()
    {
        _prevPosition = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        var deltaPos = transform.position - _prevPosition;

        _goalSpringPos = deltaPos.y > Mathf.Epsilon ? 1 : -1;
        _goalSpringPos = Mathf.Abs(deltaPos.y) < Mathf.Epsilon ? 0 : deltaPos.y;

        SpringUtil.CalcDampedSimpleHarmonicMotion(ref _springPosition, ref _springVelocity, _goalSpringPos, Time.deltaTime, _springFrequency, _springDamping);

        var scaleOffset = DefaultVerticalScale + VerticalSquishAmount * _springPosition;
        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleOffset,
            transform.localScale.z);

        NotifyListeners(_springPosition);

        _prevPosition = transform.position;
    }

    private void NotifyListeners(float springValue)
    {
        foreach(var listener in _listeners)
        {
            listener.Invoke(springValue);
        }
    }

    public void AddListener(OnSpringValue listener)
    {
        _listeners.Add(listener);
    }

    public void RemoveListener(OnSpringValue listener)
    {
        _listeners.Remove(listener);
    }
}
