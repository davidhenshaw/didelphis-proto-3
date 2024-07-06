using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Camera mainCamera;
    public float preferredZPos = 0;

    public Sprite DefaultCursor;
    public Sprite MouseDownCursor;
    public Sprite RotationCursor;

    SpriteRenderer _cursorSprite;

    GameObject _heldObj;
    IDraggable _dragObj;
    IRotate _rotateObj;

    Vector3 WorldPosition;

    [Space]
    //Sound dependencies
    [Header("Sounds")]
    AudioSource _audioSource;
    [SerializeField] AudioClip sfx_grab;
    [SerializeField] AudioClip sfx_release;
    [SerializeField] AudioClip sfx_rotate;

    private void Awake()
    {
        UnityEngine.Cursor.visible = false;

        _audioSource = GetComponent<AudioSource>();
        _cursorSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        _cursorSprite.sprite = DefaultCursor;
    }

    private void OnDisable()
    {
        UnityEngine.Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = preferredZPos;
        WorldPosition = worldPos;
        transform.position = worldPos;

        HandleInputs();
    }

    Collider2D GetMouseRaycast()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //var ray = new Ray(camera.transform.position, WorldPosition - camera.transform.position);
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(ray, 100, LayerMask.GetMask("Default"));

        return hitInfo.collider;
    }

    void HandleInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var collider = GetMouseRaycast();
            if (collider && collider.TryGetComponent(out IDraggable draggable))
            {
                _audioSource.PlayOneShot(sfx_grab);
                _heldObj = collider.gameObject;
                _dragObj = draggable;
                _heldObj.TryGetComponent<IRotate>(out _rotateObj);

                _dragObj.OnDragStart(this.transform);
                _cursorSprite.sprite = MouseDownCursor;
            }
        }

        if (_heldObj == null)
            return;

        _dragObj.OnDrag();

        if(Input.GetButtonDown("RotateCW"))
        {
            _rotateObj?.Rotate(Item.RotationType.ClockWise); 
            _audioSource.PlayOneShot(sfx_rotate);
        }

        if(Input.GetButtonDown("RotateCCW"))
        {
            _rotateObj?.Rotate(Item.RotationType.CounterClockWise); 
            _audioSource.PlayOneShot(sfx_rotate);
        }
 
 
        if (Input.GetMouseButtonUp(0))
        {
            _audioSource.PlayOneShot(sfx_release);
            _dragObj.OnDrop();

            _heldObj = null;
            _dragObj = null;
            _rotateObj = null;

            _cursorSprite.sprite = DefaultCursor;
        }
    }
}