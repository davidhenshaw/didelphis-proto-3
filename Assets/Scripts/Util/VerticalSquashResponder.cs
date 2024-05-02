using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalSquashResponder : MonoBehaviour, ISpringResponder
{
    public float VerticalSquishAmount = 0.5f;
    public float DefaultVerticalScale = 1;

    public bool autoSetScale = true;
    private Vector3 _origScale;


    private void Start()
    {
        _origScale = transform.localScale;
        if(autoSetScale)
            DefaultVerticalScale = transform.localScale.y;
    }

    public void OnSpringRemoved()
    {
        transform.localScale = _origScale;
    }

    public void OnSpringValue(float springValue)
    {
        var scaleOffset = DefaultVerticalScale + VerticalSquishAmount * springValue;
        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleOffset,
            transform.localScale.z);
    }
}
