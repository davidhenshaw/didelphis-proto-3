using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridHighlightView : MonoBehaviour, IView<GridHighlightModel>
{
    public abstract void UpdateViewWithModel(GridHighlightModel model);

    protected bool HasHoles(ICollection<Vector3Int> cells, out Vector3Int[] holes)
    {
        const int PREFERRED_Z = 0;
        holes = new Vector3Int[0];
        List<Vector3Int> holesTemp = new List<Vector3Int>();
        if (cells.Count < 4)
            return false;

        int maxX=0, maxY=0, minX = 0, minY = 0;
        foreach (var cell in cells)
        {
            maxX = Mathf.Max(maxX, cell.x +1);
            maxY = Mathf.Max(maxY, cell.y + 1);

            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
        }

        BoundsInt bounds = new BoundsInt(minX, minY, 1, Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY), 1);

        foreach(var boundsPos in bounds.allPositionsWithin)
        {
            var cellPos = new Vector3Int(boundsPos.x, boundsPos.y, PREFERRED_Z);
            //If the "cells" array contains this point, it's obviously not a hole
            if(cells.Contains(cellPos)) 
                continue;

            if(IsHole(cellPos, cells, bounds))
                holesTemp.Add(cellPos);
        }

        holes = holesTemp.ToArray();
        return holes.Length > 0;
    }

    /// <summary>
    /// Returns true if each cell shares at least one face with another cell
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    protected bool IsContiguous(ICollection<Vector3Int> cells)
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

    protected bool IsHole(Vector3Int cell, ICollection<Vector3Int> space, BoundsInt bounds)
    {
        const int PREFERRED_Z = 0;
        const int BOUNDS_PREFERRED_Z = 1;
        Vector3Int[] offsets = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
        };

        for(int i = 0; i < offsets.Length; i++)
        {
            var currentOffset = offsets[i];
            var currPos = cell + currentOffset;
            currPos.z = BOUNDS_PREFERRED_Z;

            bool borderFound = false;
            while(bounds.Contains(currPos))
            {
                currPos.z = PREFERRED_Z;
                if(space.Contains(currPos))
                    borderFound = true;

                currPos += currentOffset;
            }

            if (!borderFound)
                return false;
        }

        return true;
    }
}

[System.Serializable]
public struct GridHighlightModel
{
    public Vector3Int[] highlightedCells;
    public bool isValid;
    public Grid grid;
}

