using System.Collections.Generic;
using UnityEngine;

public static class ContainerUtil
{
    public static void SnapToCell(IGridContainable item, Vector2Int cell, Grid containerGrid)
    {
        var targetPosition = containerGrid.GetCellCenterWorld((Vector3Int)cell);

        //Actually move the item
        item.Owner.transform.position = targetPosition - item.AnchorLocalPosition;
        return;
    }

    public static bool IsContiguous(ICollection<Vector3Int> cells)
    {
        if (cells.Count <= 1)
            return true;

        foreach(var cell in cells)
        {
            bool sharesFace = cells.Contains(cell + Vector3Int.up) ||
                cells.Contains(cell + Vector3Int.down) ||
                cells.Contains(cell + Vector3Int.right) ||
                cells.Contains(cell + Vector3Int.left);

            if (!sharesFace)
                return false;
        }

        return true;
    }

    public static bool IsContiguous(ICollection<Vector2Int> cells)
    {
        if (cells.Count <= 1)
            return true;

        foreach(var cell in cells)
        {
            bool sharesFace = cells.Contains(cell + Vector2Int.up) ||
                cells.Contains(cell + Vector2Int.down) ||
                cells.Contains(cell + Vector2Int.right) ||
                cells.Contains(cell + Vector2Int.left);

            if (!sharesFace)
                return false;
        }

        return true;
    }

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
