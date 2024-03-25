using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemContainer : MonoBehaviour, IContainer
{
    [SerializeField]
    [Tooltip("A tilemap that determines which slots in this container are valid")]
    private Tilemap _tilemap;

    private Grid _grid;

    private int MaxSlotRange = 15;
    private Dictionary<Vector2Int, IContainable> _cells = new Dictionary<Vector2Int, IContainable>();

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

    public void OnDrop(IContainable draggable)
    {
        SnapToNearest(draggable);
    }

    public void SnapToNearest(IContainable item)
    {
        //Get nearest cell to anchor
        var nearestCell = _grid.WorldToCell(item.AnchorLocalOffset + item.Owner.transform.position);
        var nearestPos = _grid.GetCellCenterWorld(nearestCell);

        //Check if item can be inserted at cell position
        if (!TryAddItem(item, (Vector2Int)nearestCell))
        {
            _audioSource.PlayOneShot(sfx_itemRejected);
            return;
        }

        //Move item to grid position
        item.Owner.transform.position = nearestPos - item.AnchorLocalOffset;
        _audioSource.PlayOneShot(sfx_itemAdded);
    }

    public bool TryAddItem(IContainable item, Vector2Int insertPos)
    {
        if (!CanAddItem(item, insertPos))
            return false;

        Vector2Int[] relativePositions = item.GetCellRelativePositions();

        foreach(Vector2Int relativePos in relativePositions)
        {
            _cells.Add(relativePos + insertPos, item);
        }

        item.Container = this;
        return true;
    }

    public bool TryRemoveItem(IContainable item)
    {
        if (!_cells.ContainsValue(item))
        {
            Debug.LogWarning($"Item container {this} does not contain item {item}");
            return false;
        }

        var occupiedCells = _cells.Keys.Where((pos) =>
        {
            return _cells[pos].Equals(item);
        }).ToArray();

        foreach (Vector2Int pos in occupiedCells)
        {
            _cells.Remove(pos);
        }

        item.Container = null;
        _audioSource.PlayOneShot(sfx_itemRemoved);
        return true;
    }

    public bool CanAddItem(IContainable item, Vector2Int insertPos)
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
        return IsCellValid(cellPos) && !_cells.ContainsKey(cellPos);
    }

    public bool IsCellValid(Vector2Int cellPos)
    {
        return _tilemap.GetTile((Vector3Int)cellPos) != null;
    }

}
