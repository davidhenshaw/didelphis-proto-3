using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : SimpleDraggable, IGridContainable
{
    public static readonly float[][] ROTATION_NEG_90_MATRIX =
    {
        new float[]{ 0, -1},
        new float[]{ 1, 0}
    };

    public static readonly float[][] ROTATION_90_MATRIX = {
        new float[]{ 0, 1},
        new float[]{ -1, 0}
    };

    public static readonly float[][] IDENTITY =
    {
        new float[]{1,0},
        new float[]{0,1}
    };

    public event Action Disabled;

    public GameObject Owner => gameObject;
    public IGridContainer Container { get; set; }
    /// <summary>
    /// Local position (unity units) of this item's anchor cell
    /// </summary>
    public Vector3 AnchorLocalPosition { get; private set; }

    public Vector3 AnchorWorldPosition
    {
        get
        {
            //return AnchorLocalPosition + Owner.transform.position;
            return _slotMapGrid.GetCellCenterWorld((Vector3Int)_anchorCell);
        }
    }

    [SerializeField]
    [Tooltip("A tilemap that determines how much space this item takes up in a container")]
    private Tilemap _slotMap;

    /// <summary>
    /// Position of all item's cells relative to the anchor position
    /// </summary>
    private List<Vector2Int> _relativePos = new List<Vector2Int>();
    /// <summary>
    /// Reference point for all item's cell positions
    /// </summary>
    private Vector2Int _anchorCell;
    private Grid _slotMapGrid;
    protected Collider2D Collider;

    private bool appQuitting = false;

    [SerializeField]
    protected ContactFilter2D _contactFilter;

    private void Start()
    {
        Application.quitting += () => appQuitting = true;
        //Make sure the tile map is as small as it can be
        _slotMap.CompressBounds();

        _slotMapGrid = GetComponentInChildren<Grid>();
        Collider = GetComponent<Collider2D>();

        BoundsInt bounds = _slotMap.cellBounds;
        bool anchorFound = false;

        //Record the relative positions to the anchor tile
        // (the anchor tile is the first tile we look at)
        foreach(Vector3Int pos in bounds.allPositionsWithin)
        {
            if(!_slotMap.GetTile(pos))
            {
                continue;
            }

            if(!anchorFound)
            {
                anchorFound = true;
                _anchorCell = (Vector2Int)pos;
                AnchorLocalPosition = _slotMapGrid.GetCellCenterLocal((Vector3Int)_anchorCell);
            }

            // Calculate current cell's position offset and add to list
            _relativePos.Add((Vector2Int)pos - _anchorCell);
        }
    }

    public override void OnDrop()
    {
        base.OnDrop();

        //Find a container that overlaps this item
        var containers = new Collider2D[1];
        if(Collider.OverlapCollider(_contactFilter, containers) > 0)
        {
            if (containers[0].TryGetComponent(out IGridContainer container))
            {
                container.OnDrop(this);
            }
        }
        else
            Debug.Log("No container found");
    }

    public override void OnDragStart()
    {
        base.OnDragStart();
        if(Container != null)
        {
            Container.OnPick(this);
        }

    }

    [ContextMenu("Rotate CW")]
    public void RotateCW()
    {
        Rotate(RotationType.ClockWise);
    }

    public void Rotate(RotationType rotationType)
    {
        float rotationDegrees = 0;
        float[][] rotMatrix = IDENTITY;
        switch (rotationType)
        {
            case RotationType.ClockWise:
                rotationDegrees = -90;
                rotMatrix = ROTATION_90_MATRIX;
                break;
            case RotationType.CounterClockWise:
                rotationDegrees = 90;
                rotMatrix = ROTATION_NEG_90_MATRIX;
                break;
        }

        //If this item is already in a container, don't rotate it
        if (Container != null)
            return;

        //Rotate the cell positions
        List<Vector2Int> newPositions = new List<Vector2Int>();
        foreach(var cell in _relativePos)
        {
            var newX = Mathf.FloorToInt(cell.x * rotMatrix[0][0] + cell.y * rotMatrix[0][1]);
            var newY = Mathf.FloorToInt(cell.x * rotMatrix[1][0] + cell.y * rotMatrix[1][1]);

            newPositions.Add(new Vector2Int(newX, newY));
        }

        _relativePos = newPositions;
        //Rotate the transform
        transform.RotateAround(AnchorWorldPosition, Vector3.forward, rotationDegrees);
        RecalculateAnchorWorldPos();
    }

    private void RecalculateAnchorWorldPos()
    {
        AnchorLocalPosition = _slotMapGrid.GetCellCenterWorld((Vector3Int)_anchorCell) - transform.position;
    }

    public Vector2Int[] GetCellRelativePositions()
    {
        return _relativePos.ToArray();
    }

    private void OnDisable()
    {
        if(!appQuitting)
            Disabled?.Invoke();   
    }

    public enum RotationType
    {
        ClockWise, 
        CounterClockWise,
    }
}

