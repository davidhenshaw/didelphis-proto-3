using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemContainer : MonoBehaviour, IGridContainer
{
    [SerializeField]
    [Tooltip("A tilemap that determines which slots in this container are valid")]
    private Tilemap _tilemap;

    private Grid _grid;

    public Dictionary<Vector2Int, IGridContainable> Cells { get; } = new Dictionary<Vector2Int, IGridContainable>();

    [Header("SFX")]
    public AudioSource _audioSource;
    public AudioClip sfx_itemRejected;
    public AudioClip sfx_itemAdded;
    public AudioClip sfx_itemRemoved;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _tilemap = GetComponentInChildren<Tilemap>();
        _grid = GetComponentInChildren<Grid>();
    }

    public void OnDrop(IGridContainable draggable)
    {
        if(AddAndSnapToNearest(draggable))
            _audioSource.PlayOneShot(sfx_itemAdded);
    }

    public bool AddAndSnapToNearest(IGridContainable item)
    {
        //Get nearest cell to anchor
        var nearestCell = _grid.WorldToCell(item.AnchorLocalPosition + item.Owner.transform.position);
        var nearestPos = _grid.GetCellCenterWorld(nearestCell);

        //Check if item can be inserted at cell position
        if (!TryAddItem(item, (Vector2Int)nearestCell))
        {
            _audioSource.PlayOneShot(sfx_itemRejected);
            return false;
        }

        //Move item to grid position
        item.Owner.transform.position = nearestPos - item.AnchorLocalPosition;
        _audioSource.PlayOneShot(sfx_itemAdded);
        return true;
    }

    public void SnapToCell(IGridContainable item, Vector2Int cell)
    {
        var targetPosition = _grid.GetCellCenterWorld((Vector3Int)cell);

        //Actually move the item
        item.Owner.transform.position = targetPosition - item.AnchorLocalPosition;
        return;
    }

    public bool TryAddItem(IGridContainable item, Vector2Int insertPos)
    {
        if (!CanAddItem(item, insertPos))
            return false;

        Vector2Int[] relativePositions = item.GetCellRelativePositions();

        foreach(Vector2Int relativePos in relativePositions)
        {
            Cells.Add(relativePos + insertPos, item);
        }

        item.Container = this;
        //SnapToCell(item, insertPos);
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

        item.Container = null;
        return true;
    }

    public MovementResult CheckAddItem(IGridContainable item, Vector2Int insertPos)
    {
        MovementResult result = new MovementResult();
        List<IGridContainable> blockers = new List<IGridContainable>();
        //Check if anchor position is free
        if (!IsCellFree(insertPos))
        {
            result.CanMove = false;
            return result;
        }

        Vector2Int[] relativePostions = item.GetCellRelativePositions();
        foreach(Vector2Int cellOffset in relativePostions)
        {
            if(IsCellFree(cellOffset + insertPos))
                continue;
            else
            {
                //Record which cell was blocking the movement
                if (Cells.TryGetValue(cellOffset + insertPos, out IGridContainable blockingItem))
                    blockers.Add(blockingItem);

                result.CanMove = false;
            }
        }

        result.blockers = blockers.ToArray();
        return result;
    }

    public bool CanAddItem(IGridContainable item, Vector2Int insertPos)
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

    public bool IsCellFree(Vector2Int cellPos)
    {
        return IsCellValid(cellPos) && !Cells.ContainsKey(cellPos);
    }

    public bool IsCellValid(Vector2Int cellPos)
    {
        return _tilemap.GetTile((Vector3Int)cellPos) != null;
    }

    public Vector2Int GetAnchorCell(IGridContainable item)
    {
        return (Vector2Int)_grid.WorldToCell(item.AnchorWorldPosition);
    }

    public void OnPick(IGridContainable containable)
    {
        _audioSource.PlayOneShot(sfx_itemRemoved);
        TryRemoveItem(containable);
    }

    private void OnDestroy()
    {
        var toDestroy = Cells.Values.Select((item) => { return item.Owner; });
        foreach(GameObject obj in toDestroy)
        {
            Destroy(obj);
        }
    }
}

public struct MovementResult
{
    public IGridContainable[] blockers;
    public bool CanMove;
}

public static class ContainerUtil
{
    public static int MoveAllItems(IGridContainer container, Vector2Int offset)
    {
        HashSet<IGridContainable> seenItems = new HashSet<IGridContainable>();
        int moved = 0;
        foreach(var pos in new List<Vector2Int>(container.Cells.Keys))
        {
            //The original item may have moved. So we try get value to see if the original item's position is even there
            if (!container.Cells.TryGetValue(pos, out IGridContainable item))
                continue;
            if (seenItems.Contains(item)) // The cells dictionary has unique keys but non-unique values. So it's possible we see the same item twice
                continue;

            seenItems.Add(item);

            container.TryRemoveItem(item);
            var itemAnchor = container.GetAnchorCell(item);
            if (!container.TryAddItem(item, itemAnchor + offset))
            {//If this item could not be moved by the offset, put it back
                container.TryAddItem(item, itemAnchor);
                continue;
            }
            else
            {
                moved++;
            }
        }

        return moved;
    }
}
