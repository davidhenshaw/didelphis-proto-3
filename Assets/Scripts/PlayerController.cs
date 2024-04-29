using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Camera camera;
    public float preferredZPos = 0;

    public Sprite DefaultCursor;
    public Sprite MouseDownCursor;
    public Sprite RotationCursor;

    SpriteRenderer _cursorSprite;

    Item _heldObj;
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
        camera = Camera.main;
        UnityEngine.Cursor.visible = false;

        _audioSource = GetComponent<AudioSource>();
        _cursorSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        _cursorSprite.sprite = DefaultCursor;
    }

    private void OnDisable()
    {
        UnityEngine.Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = preferredZPos;
        WorldPosition = worldPos;
        transform.position = worldPos;

        HandleInputs();
    }

    Collider2D GetMouseRaycast()
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        //var ray = new Ray(camera.transform.position, WorldPosition - camera.transform.position);
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(ray, 100, LayerMask.GetMask("Default"));

        return hitInfo.collider;
    }

    void HandleInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var collider = GetMouseRaycast();
            if (collider && collider.TryGetComponent(out Item obj))
            {
                _audioSource.PlayOneShot(sfx_grab);
                obj.SetDragTarget(this.transform);
                obj.OnDragStart();
                _heldObj = obj;
                _cursorSprite.sprite = MouseDownCursor;
            }
        }

        if (_heldObj == null)
            return;

        _heldObj.OnDrag();

        if(Input.GetButtonDown("RotateCW"))
        {
            _heldObj.Rotate(Item.RotationType.ClockWise);
            _audioSource.PlayOneShot(sfx_rotate);
        }

        if(Input.GetButtonDown("RotateCCW"))
        {
            _heldObj.Rotate(Item.RotationType.CounterClockWise);
            _audioSource.PlayOneShot(sfx_rotate);
        }
 
 
        if (Input.GetMouseButtonUp(0))
        {
            _audioSource.PlayOneShot(sfx_release);
            _heldObj.OnDrop();
            _heldObj = null;

            _cursorSprite.sprite = DefaultCursor;
        }
    }
}