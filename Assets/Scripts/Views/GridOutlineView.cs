using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridOutlineView : MonoBehaviour, IView<GridOutlineModel>
{
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private GridOutlineModel _gridOutlineModel;

    private void Awake()
    {
        UIEvents.OnGridPlacementPreview += UpdateViewWithModel;
    }

    private void Update()
    {
        UpdateWithTestModel();
    }

    [ContextMenu("Sync with test model")]
    public void UpdateWithTestModel()
    {
        UpdateViewWithModel(_gridOutlineModel);
    }

    public void UpdateViewWithModel(GridOutlineModel model)
    {
        if (!_grid.Equals(model.grid))
            return;

        var positions = GetPerimeterVertices(model.highlightedCells);

        if (positions == null)
            return;

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }

    /// <summary>
    /// Returns true if each cell shares at least one face with another cell
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private bool IsContiguous(ICollection<Vector3Int> cells)
    {
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
    /// <summary>
    /// Given an array of <paramref name="cells"/>, returns the perimeter vertices in an order where a line can be drawn through them. <br/>
    /// Returns null if the cells are not contiguous
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private Vector3[] GetPerimeterVertices(Vector3Int[] cells)
    {
        if (!IsContiguous(cells))
        {
            Debug.LogWarning("Group of cells is not contiguous and will not be rendered.");
            return null;
        }

        Dictionary<Vector3, int> vertexCounts = new Dictionary<Vector3, int>();
        float xOffset = _grid.cellSize.x * 0.5f;
        float yOffset = _grid.cellSize.y * 0.5f;

        ///Keep track of how many times we generate the same vertices
        void CountVertex(Vector3 vertex)
        {
            if(vertexCounts.ContainsKey(vertex))
            {
                if(vertexCounts[vertex]++ >= 4) //if a vertex shares 4 cells, it is internal to the shape
                {
                    vertexCounts.Remove(vertex); // We only care about perimeter vertices, so remove it
                }
            }
            else
            {
                vertexCounts.Add(vertex, 1);
            }
        }

        foreach(var cell in cells)
        {
            var pointNW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset, 0);
            var pointNE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset, 0);
            var pointSW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset * -1, 0);
            var pointSE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset * -1, 0);

            pointNW.z = pointNE.z = pointSW.z = pointSE.z = 0;

            CountVertex(pointNW);
            CountVertex(pointNE);
            CountVertex(pointSE);
            CountVertex(pointSW);

        }
        
        List<Vector3> orderedList = new();
        List<Vector3> remainingVertices = new List<Vector3>(vertexCounts.Keys);
        Vector3 currVertex = Vector3.zero;

        while(HasNext(remainingVertices, currVertex, _grid.cellSize, out Vector3 nextVertex))
        {
            orderedList.Add(currVertex);

            remainingVertices.Remove(nextVertex);
            currVertex = nextVertex;
        }

        return orderedList.ToArray();
    }

    /// <summary>
    /// Checks in <paramref name="vertices"/> for a vertex above, right, left, and below exactly <paramref name="stepSize"/> away as the <paramref name="nextVertex"/>
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="currVertex"></param>
    /// <param name="stepSize"></param>
    /// <param name="nextVertex"></param>
    /// <returns></returns>
    private bool HasNext(List<Vector3> vertices, Vector3 currVertex, Vector2 stepSize, out Vector3 nextVertex)
    {
        nextVertex = Vector3.zero;
        if (vertices.Count == 0)
        {
            return false;
        }

        //Check for adjacent vertices in clockwise order
        Vector3[] nextChecks = {
            currVertex + new Vector3(0, stepSize.y), //up
            currVertex + new Vector3(stepSize.x, 0), //right
            currVertex + new Vector3(0, -1 * stepSize.y), //down
            currVertex + new Vector3( -1 *stepSize.x, 0) //left
        };

        for (int i = 0; i < nextChecks.Length; i++)
        {
            if (vertices.Contains(nextChecks[i] ))
            {
                nextVertex = nextChecks[i];
                return true;
            }
        }

        return false;
    }
}

public interface IView<TModel>
{
    void UpdateViewWithModel(TModel model);
}

[System.Serializable]
public struct GridOutlineModel
{
    public Vector3Int[] highlightedCells;
    public Grid grid;
}
