using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : SimpleDraggable, IGridContainable, IBroadcastRotation
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
    public event IBroadcastRotation.BroadcastRotationDelegate Rotated;

    public GameObject Owner => gameObject;
    public Orientation Orientation { get; private set; }

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

    public ItemAttribute Attributes;

    public List<ItemProperty> Properties { get; private set; }

    [SerializeField]
    [Tooltip("A tilemap that determines how much space this item takes up in a container")]
    private Tilemap _slotMap;

    /// <summary>
    /// Position of all item's cells relative to the anchor position
    /// </summary>
    private List<Vector2Int> _relativePos = new List<Vector2Int>();
    /// <summary>
    /// Reference point on local grid for all item's cell positions
    /// </summary>
    private Vector2Int _anchorCell;
    private Grid _slotMapGrid;
    protected Collider2D Collider;

    private bool appQuitting = false;

    private IGridContainer _tempContainer;
    [SerializeField]
    protected ContactFilter2D _contactFilter;


    private void Awake()
    {
        Properties = new List<ItemProperty>();
        foreach(var property in GetComponents<ItemProperty>())
        {
            Properties.Add(property);
        }

        foreach(var rotationListener in GetComponentsInChildren<IRotationListener>())
        {
            Rotated += rotationListener.OnRotationChanged;
        }
    }

    protected override void Start()
    {
        base.Start();
        Application.quitting += () => appQuitting = true;
        //Make sure the tile map is as small as it can be
        _slotMap.CompressBounds();

        _slotMapGrid = GetComponentInChildren<Grid>();
        Collider = GetComponent<Collider2D>();

        RecalculateAnchor();
    }

    public override void OnDrop()
    {
        base.OnDrop();

        _tempContainer?.OnHoverEnd();

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

    public override void OnDragStart(Transform target)
    {
        base.OnDragStart( target);
        if(Container != null)
        {
            Container.OnPick(this);
        }

    }

    public override void OnDrag()
    {
        base.OnDrag();

        //Find a container that overlaps this item
        var containers = new Collider2D[1];
        if(Collider.OverlapCollider(_contactFilter, containers) > 0)
        {
            if (containers[0].TryGetComponent(out IGridContainer newContainer))
            {
                if (_tempContainer == null)
                    _tempContainer = newContainer;

                if(newContainer.Equals(_tempContainer))
                {
                    _tempContainer.OnHover(this);
                }
                else
                {
                    _tempContainer.OnHoverEnd();
                    _tempContainer = newContainer;
                    _tempContainer.OnHover(this);
                }
            }
        }
        else
        {
            if (_tempContainer != null)
            {
                _tempContainer.OnHoverEnd();
                _tempContainer = null;
            }
        }
    }

    public void Rotate(RotationType rotationType)
    {
        float oldRotation = (int)Orientation * -90;
        float rotationDegrees = 0;
        float[][] rotMatrix = IDENTITY;
        switch (rotationType)
        {
            case RotationType.ClockWise:
                rotationDegrees = -90;
                rotMatrix = ROTATION_90_MATRIX;
                Orientation = (Orientation) (((int)this.Orientation + 1) % 4);
                break;
            case RotationType.CounterClockWise:
                rotationDegrees = 90;
                rotMatrix = ROTATION_NEG_90_MATRIX;
                Orientation = (Orientation) (((int)this.Orientation + 3) % 4);
                break;
        }

        //If this item is already in a container, don't rotate it
        if (Container != null)
            return;

        //Rotate the cell data
        List<Vector2Int> newPositions = new List<Vector2Int>();
        Dictionary<Vector3Int, TileBase> newTiles = new Dictionary<Vector3Int, TileBase>();
        foreach(var cell in _relativePos)
        {
            var newX = Mathf.FloorToInt(cell.x * rotMatrix[0][0] + cell.y * rotMatrix[0][1]);
            var newY = Mathf.FloorToInt(cell.x * rotMatrix[1][0] + cell.y * rotMatrix[1][1]);
            var newPosition = new Vector2Int(newX, newY);

            newPositions.Add(newPosition);

            // Remove old tiles and cache their new positions
            var localGridCell = (Vector3Int)(_anchorCell + cell);
            var newGridCell = (Vector3Int)(_anchorCell + newPosition);
            var tile = _slotMap.GetTile(localGridCell);
            newTiles.Add(newGridCell, tile);
            _slotMap.SetTile(localGridCell, null);
        }

        // Place previous tiles at new cell positions
        foreach(var newCell in newTiles.Keys)
        {
            _slotMap.SetTile(newCell, newTiles[newCell]);
        }
 
        _relativePos = newPositions;
        //Rotate the sprite
        var spriteTf = GetComponentInChildren<SpriteRenderer>().transform;

        spriteTf.RotateAround(AnchorWorldPosition, Vector3.forward, rotationDegrees);
        spriteTf.eulerAngles = new Vector3(
            spriteTf.eulerAngles.x,
            spriteTf.eulerAngles.y,
            (int)Orientation * -90
            );
        RecalculateAnchorWorldPos();

        Rotated?.Invoke(oldRotation, (int)Orientation * -90);
    }

    private void RecalculateAnchorWorldPos()
    {
        AnchorLocalPosition = _slotMapGrid.GetCellCenterWorld((Vector3Int)_anchorCell) - transform.position;
    }

    public Vector2Int[] GetCellRelativePositions()
    {
        return _relativePos.ToArray();
    }

    public void RemoveLocalCell(Vector2Int cell)
    {
        _relativePos.Remove(cell);
        _slotMap.SetTile((Vector3Int)(_anchorCell+cell), null);
    }

    public void RecalculateAnchor()
    {
        BoundsInt bounds = _slotMap.cellBounds;
        bool anchorFound = false;

        _relativePos.Clear();
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

[System.Flags]
public enum ItemAttribute
{
    None, Heavy, Fragile, Crushable
}

public enum ItemStatus
{
    Broken
}

