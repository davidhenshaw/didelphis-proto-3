using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemCluster : Item, IGridContainable, IGridContainer
{
    public Dictionary<Vector2Int, IGridContainable> Cells { get; private set; } = new();

    private List<Vector2Int> _localCellPositions = new();
    private IGridContainable _anchorItem;
    private Tilemap _tilemap;

    private Collider2D _collider;

    private IBucket _tempDropBucket;
    private int MAX_COLLIDER_DEPTH = 15;


    public bool addChildrenOnStart = true;

    protected override void Awake()
    {
        base.Awake();
        _tilemap = GetComponentInChildren<Tilemap>();
        _draggable = GetComponent<SimpleDraggable>();

        _draggable.DragStarted += OnDragStart;
        _draggable.DragFinished += OnDrop;
        _draggable.OnDragCallback += OnDrag;
    }

    protected override void Start()
    {
        base.Start();
        if(addChildrenOnStart)
        {
            var allItems = GetComponentsInChildren<Item>();

            foreach (var item in allItems)
            {
                var insertPosition = _tilemap.WorldToCell(item.AnchorWorldPosition);
                TryAddItem(item, (Vector2Int)insertPosition);
            }
        }

        RecalculateBorderPositions();
    }

    private void OnDisable()
    {
        _draggable.DragStarted -= OnDragStart;
        _draggable.DragFinished -= OnDrop;
        _draggable.OnDragCallback -= OnDrag;
    }

    public override Vector2Int[] GetCellRelativePositions()
    {
        var relativePositions = new HashSet<Vector2Int>();

        foreach (var item in Cells.Keys)
        {
            relativePositions.Add(item - _anchorGridCell);
        }

        return relativePositions.ToArray();
    }
    
    public override void RecalculateBorderPositions()
    {
        var localCellPos = GetCellRelativePositions();

        _borderPos.Clear();
        HashSet<Vector2Int> positionSet = new HashSet<Vector2Int>();
        //Record the relative positions to the anchor tile
        // (the anchor tile is the first tile we look at)
        foreach(Vector2Int pos in localCellPos)
        {
            // Calculate current cell's position offset and add to list
            // Get adjacent cells
            var adjacents = ContainerController.GetAdjacents(pos);
            foreach(var cell in adjacents)
            {
                positionSet.Add((Vector2Int)cell);
            }
        }        
        
        //Remove positions on the shape from the set
        // to reveal only border positions
        positionSet.ExceptWith(new HashSet<Vector2Int>(localCellPos));
        _borderPos = new List<Vector2Int>(positionSet);

    }

    public override void Rotate(Item.RotationType rotationType)
    {
        base.Rotate(rotationType);
    }

    public bool CanAddItem(IGridContainable item, Vector2Int insertPos)
    {
        if(Cells.Count == 0)
        {
            return true;
        }

        var cellList = new List<Vector2Int>(Cells.Keys);
        //new items must be contiguous with existing items in the cluster
        foreach(var localCell in item.GetCellRelativePositions())
        {
            var containerCell = localCell + insertPos;

            if (cellList.Contains(containerCell))
            {
                Debug.Log("Item overlaps container cell");
                return false;
            }

            cellList.Add(containerCell);
        }

        return ContainerUtil.IsContiguous(cellList);
    }

    public bool TryAddItem(IGridContainable item, Vector2Int insertPos)
    {

        if(Cells.Count == 0)
        {
            _anchorItem = item;
            _anchorGridCell = insertPos;
        }

        //new items must be contiguous with existing items in the cluster
        foreach(var relativeCell in item.GetCellRelativePositions())
        {
            var containerCell = relativeCell + insertPos;

            if (Cells.ContainsKey(containerCell))
            {
                Debug.Log("Item overlaps container cell");
                return false;
            }

            Cells.Add(containerCell, item);
        }

        PrepareForAdd(item);
        ContainerUtil.SnapToCell(item, insertPos, _tilemap.layoutGrid);
        CopyTileMapDatas(item, insertPos);

        if (Cells.Count == 0)
        {
            _anchorItem = null;
        }


        return true;
    }

    public bool TryRemoveItem(IGridContainable item)
    {
        if (!Cells.ContainsValue(item))
        {
            Debug.LogWarning($"Item container {this} does not contain item {item}");
            return false;
        }

        var occupiedCells = Cells.Keys.Where((pos) =>
        {
            return Cells[pos].Equals(item);
        }).ToArray();

        foreach (Vector2Int pos in occupiedCells)
        {
            Cells.Remove(pos);
        }

        PrepareForRemove(item);
        return true;
    }
    private void CopyTileMapDatas(IGridContainable item, Vector2Int insertPos)
    {
        foreach (var pos in item.GetCellRelativePositions())
        {
            var itemTilePos = (Vector3Int)(pos + item.LocalGridAnchor);
            Vector3Int containerPos = (Vector3Int)(pos + insertPos);

            var tile = item.GetLayoutTilemap().GetTile(itemTilePos);
            _tilemap.SetTile(containerPos, tile);
        }
    }

    public Vector2Int GetAnchorCell(IGridContainable item)
    {
        return (Vector2Int)_anchorItem.GetLayoutTilemap().WorldToCell(item.AnchorWorldPosition);
    }

    public void OnPick(IGridContainable containable)
    {
        throw new System.NotImplementedException();
    }

    public void PrepareForAdd(IGridContainable item)
    {
        if (item.Equals(this))
            return;

        item.Owner.transform.SetParent(this.transform);

        if(item.Owner.TryGetComponent(out CompositeCollider2D compositeCollider))
        {
            Destroy(compositeCollider);
        }

        if(item.Owner.TryGetComponent(out Rigidbody2D rb))
        {
            Destroy(rb);
        }

        item.Container = this;
    }
    public void PrepareForRemove(IGridContainable item)
    {
        if (item.Equals(this))
            return;

        item.Owner.transform.SetParent(transform.parent);

        if(!item.Owner.TryGetComponent(out Rigidbody2D rb))
        {
            var rigidbody = item.Owner.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
            rigidbody.isKinematic = true;
        }

        if(!item.Owner.TryGetComponent(out CompositeCollider2D compositeCollider))
        {
            var collider = item.Owner.AddComponent(typeof(CompositeCollider2D)) as CompositeCollider2D;
            collider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            collider.GenerateGeometry();
            collider.generationType = CompositeCollider2D.GenerationType.Synchronous;
        }

        item.Container = null;
    }
    [ContextMenu(nameof(AddAllChildren))]
    public void AddAllChildren()
    {
        var allItems = FindObjectsByType<Item>(FindObjectsSortMode.InstanceID);

        foreach (var item in allItems)
        {
            var insertPosition = _tilemap.WorldToCell(item.AnchorWorldPosition);
            TryAddItem(item, (Vector2Int)insertPosition);
        }
    }

    [ContextMenu(nameof(RemoveAllChildren))]
    public void RemoveAllChildren()
    {
        var allItems = GetComponentsInChildren<Item>();

        foreach(var item in allItems)
        {
            TryRemoveItem(item);
        }
    }
}
