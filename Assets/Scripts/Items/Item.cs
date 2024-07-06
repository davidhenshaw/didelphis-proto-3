using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : MonoBehaviour, IGridContainable, IBroadcastRotation, IRotate
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
    private const int MAX_COLLIDER_DEPTH = 15;

    public event Action Disabled;
    public event IBroadcastRotation.BroadcastRotationDelegate Rotated;
    protected SimpleDraggable _draggable;

    public GameObject Owner => gameObject;
    public Orientation Orientation { get; private set; }

    public IGridContainer Container { get; set; }
    /// <summary>
    /// Local position (unity units) of this item's anchor cell
    /// </summary>
    public Vector3 AnchorLocalPosition => _slotMap.GetCellCenterLocal((Vector3Int)_anchorGridCell);

    public Vector3 AnchorWorldPosition => _slotMap.GetCellCenterWorld((Vector3Int)_anchorGridCell);

    public ItemAttribute Attributes;

    public List<ItemProperty> Properties { get; private set; }

    [SerializeField]
    [Tooltip("A tilemap that determines how much space this item takes up in a container")]
    public Tilemap _slotMap;

    public virtual Tilemap GetLayoutTilemap()
    {
        return _slotMap;
    }

    /// <summary>
    /// Position of all item's cells relative to the anchor position
    /// </summary>
    protected List<Vector2Int> _relativePos = new List<Vector2Int>();
    protected List<Vector2Int> _borderPos = new List<Vector2Int>();

    public Vector2Int[] BorderPositions => _borderPos.ToArray();

    /// <summary>
    /// Reference point on local grid for all item's cell positions
    /// </summary>
    protected Vector2Int _anchorGridCell;

    public Vector2Int LocalGridAnchor => _anchorGridCell;
    private Grid _slotMapGrid;
    protected Collider2D Collider;

    private bool appQuitting = false;

    private IDragBucket _tempDragBucket;
    [SerializeField]
    protected ContactFilter2D _contactFilter;

    protected virtual void Awake()
    {
        Properties = new List<ItemProperty>();
        _draggable = GetComponent<SimpleDraggable>();

        foreach(var property in GetComponents<ItemProperty>())
        {
            Properties.Add(property);
        }

        foreach(var rotationListener in GetComponentsInChildren<IRotationListener>())
        {
            Rotated += rotationListener.OnRotationChanged;
        }

        _draggable.DragStarted += OnDragStart;
        _draggable.DragFinished += OnDrop ;
        _draggable.OnDragCallback += OnDrag;

        _slotMapGrid = GetComponentInChildren<Grid>();
        Collider = GetComponent<Collider2D>();

        _slotMap.CompressBounds();
        RecalculateAnchor();
    }

    protected virtual void Start()
    {
        //Application.quitting += () => appQuitting = true;
    }

    public virtual void OnDrop(Transform target, Vector3 offset)
    {
        _tempDragBucket?.OnHoverEnd();

        //Find a container that overlaps this item
        var containers = new Collider2D[MAX_COLLIDER_DEPTH];
        var numOverlaps = Collider.OverlapCollider(_contactFilter, containers);

        //loop through and pick the first container you find
        for (int i = 0; i < numOverlaps; i++)
        {
            if (containers[i].TryGetComponent(out IDragBucket container))
            {
                container.OnDrop(_draggable);
                break;
            }
        }
    }

    public virtual void OnDragStart(Transform target, Vector3 offset)
    {
        if(!Collider)//re-cache the item's collider bc it may have changed
            Collider = GetComponent<Collider2D>();

        if(Container != null)
        {
            Container.OnPick(this);
        }

    }

    public virtual void OnDrag()
    {
        //Find a container that overlaps this item
        var containers = new Collider2D[MAX_COLLIDER_DEPTH];
        var numOverlap = Collider.OverlapCollider(_contactFilter, containers);
        if(numOverlap > 0)
        {
            for (int i = 0; i < numOverlap; i++)
            {
                if (containers[i].TryGetComponent(out IDragBucket newContainer))
                {
                    if (_tempDragBucket == null)
                        _tempDragBucket = newContainer;

                    if (newContainer.Equals(_tempDragBucket))
                    {
                        _tempDragBucket.OnHover(_draggable);
                    }
                    else
                    {
                        _tempDragBucket.OnHoverEnd();
                        _tempDragBucket = newContainer;
                        _tempDragBucket.OnHover(_draggable);
                    }

                    break;
                }
            }
        }
        else
        {
            if (_tempDragBucket != null)
            {
                _tempDragBucket.OnHoverEnd();
                _tempDragBucket = null;
            }
        }
    }

    public virtual void Rotate(RotationType rotationType)
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
            var localGridCell = (Vector3Int)(_anchorGridCell + cell);
            var newGridCell = (Vector3Int)(_anchorGridCell + newPosition);
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
        RecalculateBorderPositions();

        Rotated?.Invoke(oldRotation, (int)Orientation * -90);
    }

    public virtual Vector2Int[] GetCellRelativePositions()
    {
        return _relativePos.ToArray();
    }

    public void RemoveLocalCell(Vector2Int cell)
    {
        _relativePos.Remove(cell);
        _slotMap.SetTile((Vector3Int)(_anchorGridCell+cell), null);
    }

    public virtual void RecalculateBorderPositions()
    {
        BoundsInt bounds = _slotMap.cellBounds;

        _borderPos.Clear();
        HashSet<Vector2Int> positionSet = new HashSet<Vector2Int>();
        //Record the relative positions to the anchor tile
        // (the anchor tile is the first tile we look at)
        foreach(Vector3Int pos in bounds.allPositionsWithin)
        {
            if(!_slotMap.GetTile(pos))
            {
                continue;
            }

            // Calculate current cell's position offset and add to list
            var offsetPos = (Vector2Int)pos - _anchorGridCell;

            // Calculate border cells
            var adjacents = ContainerController.GetAdjacents(offsetPos);
            foreach(var cell in adjacents)
            {
                if (_relativePos.Contains((Vector2Int)cell))
                    continue;

                positionSet.Add((Vector2Int)cell);
            }
        }        
        
        //Remove positions on the shape from the set
        // to reveal only border positions
        positionSet.ExceptWith(new HashSet<Vector2Int>(_relativePos));
        _borderPos = new List<Vector2Int>(positionSet);

    }

    public virtual void RecalculateAnchor()
    {
        BoundsInt bounds = _slotMap.cellBounds;
        bool anchorFound = false;

        _relativePos.Clear();
        _borderPos.Clear();
        HashSet<Vector2Int> positionSet = new HashSet<Vector2Int>();
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
                _anchorGridCell = (Vector2Int)pos;
            }

            // Calculate current cell's position offset and add to list
            var offsetPos = (Vector2Int)pos - _anchorGridCell;
            _relativePos.Add(offsetPos);
            positionSet.Add(offsetPos);

            // Calculate border cells
            var adjacents = ContainerController.GetAdjacents(offsetPos);
            foreach(var adjacentCell in adjacents)
            {
                positionSet.Add((Vector2Int)adjacentCell);
            }
        }

        //Remove positions on the shape from the set
        // to reveal only border positions
        positionSet.ExceptWith(new HashSet<Vector2Int>(_relativePos));
        _borderPos = new List<Vector2Int>(positionSet);
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

