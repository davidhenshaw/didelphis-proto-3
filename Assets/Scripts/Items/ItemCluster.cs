using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemCluster : IGridContainer, IGridContainable
{
    public GameObject Owner => _anchorItem?.Owner;

    public Orientation Orientation { get; private set; }

    public IGridContainer Container { get; set; }

    public Vector3 AnchorLocalPosition => _anchorItem.AnchorLocalPosition;

    public Vector3 AnchorWorldPosition => _anchorItem.AnchorWorldPosition;

    private List<Vector2Int> _borderPos = new();
    public Vector2Int[] BorderPositions { get; }

    public Dictionary<Vector2Int, IGridContainable> Cells { get; private set; }

    private IGridContainable _anchorItem;

    public Vector2Int[] GetCellRelativePositions()
    {
        var allItems = new HashSet<IGridContainable>(Cells.Values);
        var cellPositions = new List<Vector2Int>();

        foreach(var item in allItems)
        {
            cellPositions.AddRange(item.GetCellRelativePositions());
        }

        return cellPositions.ToArray();
    }
    
    public void RecalculateBorderPositions()
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

    public Tilemap GetTilemap()
    {
        return _anchorItem?.GetTilemap();
    }

    public void Rotate(Item.RotationType rotationType)
    {
        throw new System.NotImplementedException();
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
        //new items must be contiguous with existing items in the cluster
        foreach(var localCell in item.GetCellRelativePositions())
        {
            var containerCell = localCell + insertPos;

            if (Cells.ContainsKey(containerCell))
            {
                Debug.Log("Item overlaps container cell");
                return false;
            }

            Cells.Add(containerCell, item);
        }

        item.Container = this;
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

    public Vector2Int GetAnchorCell(IGridContainable item)
    {
        return (Vector2Int)_anchorItem.GetTilemap().WorldToCell(item.AnchorWorldPosition);
    }

    public void OnPick(IGridContainable containable)
    {
        throw new System.NotImplementedException();
    }
}
