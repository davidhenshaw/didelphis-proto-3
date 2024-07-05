using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemContainer : MonoBehaviour, IGridContainer, IBucket
{
    [SerializeField]
    [Tooltip("A tilemap that determines which slots in this container are valid")]
    public Tilemap TileMap;

    [SerializeField]
    [Tooltip("A tilemap which keeps track of item tile types")]
    public Tilemap ItemTileMap;
    [SerializeField]
    private TileBase _emptyTile;

    public Dictionary<Vector2Int, IGridContainable> Cells { get; } = new Dictionary<Vector2Int, IGridContainable>();
    public int CellCapacity { get; private set; }

    [Header("UI")]
    [SerializeField]
    private GridHighlightView _view;

    [Header("SFX")]
    public AudioSource _audioSource;
    public AudioClip sfx_itemRejected;
    public AudioClip sfx_itemAdded;
    public AudioClip sfx_itemRemoved;

    private GridHighlightModel _outlineUIModel;

    public delegate void GridItemEvent(IGridContainable item);

    public event GridItemEvent ItemAdded;
    public event GridItemEvent ItemRemoved;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _outlineUIModel = new();
        _outlineUIModel.grid = TileMap.layoutGrid;
    }

    private void Start()
    {
        CountCellCapacity();
    }

    public Vector2Int[] GetCellsOfItem(IGridContainable item)
    {
        var occupiedCells = Cells.Keys.Where((pos) =>
        {
            return Cells[pos].Equals(item);
        }).ToArray();

        return occupiedCells;
    }

    private void CountCellCapacity()
    {
        BoundsInt bounds = TileMap.cellBounds;

        CellCapacity = 0;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (TileMap.GetTile(pos))
            {//if the tilemap contains a tile at this position, count it toward the capacity
                CellCapacity++;
            }
        }


    }

    public void OnHoverEnd()
    {
        _outlineUIModel.highlightedCells = new Vector3Int[0];
        _view.UpdateViewWithModel(_outlineUIModel);
    }

    public void OnHover(IGridContainable item)
    {
        var containerAnchor = (Vector3Int)GetAnchorCell(item);
        var nearestPos = TileMap.GetCellCenterWorld(containerAnchor);

        List<Vector3Int> containerCells = new List<Vector3Int>();
        foreach(var localCell in item.GetCellRelativePositions())
        {
            containerCells.Add((Vector3Int)localCell + containerAnchor);
        }

        _outlineUIModel.isValid = CanAddItem(item, (Vector2Int)containerAnchor);
        _outlineUIModel.highlightedCells = containerCells.ToArray();
        _view.UpdateViewWithModel(_outlineUIModel);
    }

    public void OnDrop(IGridContainable draggable)
    {
        OnHoverEnd();
        if(AddAndSnapToNearest(draggable))
        {
            var anchorPos = GetAnchorCell(draggable);
            ItemAdded?.Invoke(draggable);
            _audioSource.PlayOneShot(sfx_itemAdded);
        }
    }

    public bool AddAndSnapToNearest(IGridContainable item)
    {
        //Get nearest cell to anchor
        var nearestCell = GetAnchorCell(item);

        //Check if item can be inserted at cell position
        if (!TryAddItem(item, (Vector2Int)nearestCell))
        {
            _audioSource.PlayOneShot(sfx_itemRejected);
            return false;
        }

        //Move item to grid position
        ContainerUtil.SnapToCell(item, nearestCell, TileMap.layoutGrid);
        _audioSource.PlayOneShot(sfx_itemAdded);
        return true;
    }

    public void SnapToCell(IGridContainable item, Vector2Int cell)
    {
        var targetPosition = TileMap.GetCellCenterWorld((Vector3Int)cell);

        //Actually move the item
        item.Owner.transform.position = targetPosition - item.AnchorLocalPosition;
        return;
    }

    public virtual bool TryAddItem(IGridContainable item, Vector2Int insertPos)
    {
        if (!CanAddItem(item, insertPos))
            return false;

        Vector2Int[] relativePositions = item.GetCellRelativePositions();
        foreach(Vector2Int relativePos in relativePositions)
        {
            Cells.Add(relativePos + insertPos, item);
        }

        item.Container = this;
        CopyTileMapDatas(item, insertPos);
        return true;
    }

    public virtual bool TryRemoveItem(IGridContainable item)
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

        RemoveTileMapDatas(item);

        foreach (Vector2Int pos in occupiedCells)
        {
            Cells.Remove(pos);
        }


        item.Container = null;
        return true;
    }

    public MovementResult CanMoveItem(IGridContainable item, Vector2Int insertPos)
    {
        MovementResult result = new MovementResult
        {
            CanMove = true
        };
        List<IGridContainable> collisions = new List<IGridContainable>();
        //Check if anchor position is free
        if (!IsCellValid(insertPos))
        {
            result.CanMove = false;
            return result;
        }

        //Record which items you would collide with 
        Vector2Int[] relativePostions = item.GetCellRelativePositions();
        foreach(Vector2Int cellOffset in relativePostions)
        {
            var containerPosition = cellOffset + insertPos;
            bool willCollide = !IsCellFree(containerPosition) && !IsCellOwnedByItem(item, containerPosition);
            if (willCollide)
            {
                //Record which cell was blocking the movement
                if (Cells.TryGetValue(containerPosition, out IGridContainable blockingItem))
                    collisions.Add(blockingItem);

                //result.CanMove = false;
            }
        }

        result.collisions = collisions.ToArray();

        if (collisions.Count <= 0)
            return result;

        //Check if the collisions can be resolved by the item's properties
        var properties = item.Owner.GetComponents<IGridMovementValidator>();
        foreach(var property in properties)
        {
            if (!property.CanResolveCollision(collisions.ToArray()))
                result.CanMove = false;
        }    

        return result;
    }

    public virtual bool CanAddItem(IGridContainable item, Vector2Int insertPos)
    {
        //Check if anchor position is free
        if (!IsCellFree(insertPos))
            return false;

        Vector2Int[] relativePostions = item.GetCellRelativePositions();
        foreach(Vector2Int cellOffset in relativePostions)
        {
            if(IsCellFree(cellOffset + insertPos))
                continue;
            else
                return false;
        }

        return true;
    }

    public virtual bool IsCellFree(Vector2Int cellPos)
    {
        return IsCellValid(cellPos) && !Cells.ContainsKey(cellPos);
    }

    public virtual bool IsCellValid(Vector2Int cellPos)
    {
        var tile = TileMap.GetTile<ContainerTile>((Vector3Int)cellPos); 

        if(tile)
        {
            return tile.accessType == ContainerTile.AccessType.Allowed;
        }
        else
            return false;
    }

    public bool IsCellOwnedByItem(IGridContainable item, Vector2Int containerCell)
    {
        if (!Cells.TryGetValue(containerCell, out IGridContainable itemBelow))
            return false;

        return item.Equals(itemBelow);
    }

    public Vector2Int GetAnchorCell(IGridContainable item)
    {
        return (Vector2Int)TileMap.WorldToCell(item.AnchorWorldPosition);
    }

    public void OnPick(IGridContainable containable)
    {
        _audioSource.PlayOneShot(sfx_itemRemoved);
        if(TryRemoveItem(containable))
        {
            ItemRemoved?.Invoke(containable);
        }
    }

    private void CopyTileMapDatas(IGridContainable item, Vector2Int insertPos)
    {
        foreach(var pos in item.GetCellRelativePositions())
        {
            var itemTilePos = (Vector3Int)(pos + item.LocalGridAnchor);
            Vector3Int containerPos = (Vector3Int)(pos + insertPos);

            var tile = item.GetLayoutTilemap().GetTile(itemTilePos);
            ItemTileMap.SetTile(containerPos, tile);
        }
    }

    private void RemoveTileMapDatas(IGridContainable containable)
    {
        var containerPositions = GetCellsOfItem(containable);

        foreach(var pos in containerPositions)
        {
            ItemTileMap.SetTile((Vector3Int)pos, null);
        }

    }
}

public struct MovementResult
{
    public IGridContainable[] collisions;
    public bool CanMove;
    public Vector2Int DesiredPosition;
    public Vector2Int DesiredMovement;
}
