using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : SimpleDraggable, IContainable
{
    public GameObject Owner => gameObject;
    public IContainer Container { get; set; }
    /// <summary>
    /// Local position (unity units) of this item's anchor cell
    /// </summary>
    public Vector3 AnchorLocalOffset { get; private set; }


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
    protected Collider2D Collider;

    [SerializeField]
    protected ContactFilter2D _contactFilter;

    private void Start()
    {
        //Make sure the tile map is as small as it can be
        _slotMap.CompressBounds();

        var grid = GetComponentInChildren<Grid>();
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
                AnchorLocalOffset = grid.GetCellCenterLocal((Vector3Int)_anchorCell);
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
            if (containers[0].TryGetComponent(out IContainer container))
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
            Container.TryRemoveItem(this);
        }

    }

    public Vector2Int[] GetCellRelativePositions()
    {
        return _relativePos.ToArray();
    }
}
