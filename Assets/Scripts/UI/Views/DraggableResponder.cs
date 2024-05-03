using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableResponder : MonoBehaviour, ISpringResponder, IRotationListener
{

    private SpriteRenderer _spriteRenderer;
    private Color _origColor;
    [SerializeField]
    private SpriteRenderer _dropShadow;

    public float OpacityOnDrag = 0.8f;
    public float ScaleGrowth = 1.25f;
    public float MaxRotation = 30;
    private Vector3 _rotationPoint;

    private Vector3 _origRotation;
    private Vector3 _origScale;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _origColor = _spriteRenderer.color;
        _origRotation = transform.eulerAngles;
        _origScale = transform.localScale;

        if(_dropShadow != null)
            _dropShadow.enabled = false;
    }

    public void OnDragStart(Transform target, Vector3 offset)
    {
        _origColor = _spriteRenderer.color;
        _origScale = transform.localScale;

        var tempColor = _origColor;
        tempColor.a = OpacityOnDrag;
        _spriteRenderer.color = tempColor;
        transform.localScale *= ScaleGrowth;

        if(_dropShadow)
            _dropShadow.enabled = true;
    }

    public void OnDragFinished(Transform target, Vector3 offset)
    {
        _spriteRenderer.color = _origColor;
        transform.localScale = _origScale;
        if(_dropShadow)
            _dropShadow.enabled = false;
    }

    public void OnSpringValue(float springValue)
    {
        transform.eulerAngles = new Vector3(0, 0, _origRotation.z + springValue * MaxRotation * -1);
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
