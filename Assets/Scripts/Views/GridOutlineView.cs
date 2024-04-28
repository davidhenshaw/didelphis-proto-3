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

        Dictionary<Vector3, HashSet<Vector3Int>> vertexMap = new Dictionary<Vector3, HashSet<Vector3Int>>();
        float xOffset = _grid.cellSize.x * 0.5f;
        float yOffset = _grid.cellSize.y * 0.5f;

        ///Keep track of how many times we generate the same vertices
        void CountVertex(Vector3 vertex, Vector3Int cell)
        {
            if(vertexMap.ContainsKey(vertex))
            {
                vertexMap[vertex].Add(cell);
                if(vertexMap[vertex].Count >= 4) //if a vertex shares 4 cells, it is internal to the shape
                {
                    vertexMap.Remove(vertex); // We only care about perimeter vertices, so remove it
                }
            }
            else
            {
                vertexMap.Add(vertex, new HashSet<Vector3Int>{ cell });
            }
        }

        //vertex we will start traversal with
        // must be guaranteed to be on external edge of shape
        Vector3 startVertex = Vector3.zero;

        foreach(var cell in cells)
        {
            var pointNW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset, 0);
            var pointNE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset, 0);
            var pointSW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset * -1, 0);
            var pointSE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset * -1, 0);

            pointNW.z = pointNE.z = pointSW.z = pointSE.z = 0;

            CountVertex(pointNW, cell);
            CountVertex(pointNE, cell);
            CountVertex(pointSE, cell);
            CountVertex(pointSW, cell);

            //start vertex is the most northwestern point we can find on the shape
            startVertex = (pointNW.x < startVertex.x) && (pointNW.y < startVertex.y) ? pointNW : startVertex;
        }
        
        List<Vector3> orderedList = new();
        List<Vector3> remainingVertices = new List<Vector3>(vertexMap.Keys);
        Vector3 currVertex = startVertex;
        orderedList.Add(startVertex);
        remainingVertices.Remove(startVertex);

        while(HasNext(remainingVertices, vertexMap,currVertex, _grid.cellSize, out Vector3 nextVertex))
        {
            orderedList.Add(nextVertex);

            remainingVertices.Remove(nextVertex);
            currVertex = nextVertex;
        }

        return orderedList.ToArray();
    }

    /// <summary>
    /// Checks in <paramref name="vertices"/> for a vertex above, right, left, or below exactly <paramref name="stepSize"/> away as the <paramref name="nextVertex"/>
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="currVertex"></param>
    /// <param name="stepSize"></param>
    /// <param name="nextVertex"></param>
    /// <returns></returns>
    private bool HasNext(List<Vector3> vertices, Dictionary<Vector3, HashSet<Vector3Int>> vertexMap,Vector3 currVertex, Vector2 stepSize, out Vector3 nextVertex)
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
                if (!IsJumpValid(currVertex, nextChecks[i], vertexMap))
                    continue;

                nextVertex = nextChecks[i];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// For jumps to be valid, <strong>exactly one cell</strong> must be common to the <paramref name="vOrigin"/> and <paramref name="vOther"/> vertex<br/>
    /// Otherwise, the jump would either pass through the shape or jump across vertices which shouldn't be connected
    /// </summary>
    /// <param name="vOrigin"></param>
    /// <param name="vOther"></param>
    /// <param name="vertexMap"></param>
    /// <returns></returns>
    private bool IsJumpValid(Vector3 vOrigin, Vector3 vOther, Dictionary<Vector3, HashSet<Vector3Int>> vertexMap)
    {
        var originCells = new HashSet<Vector3Int>(vertexMap[vOrigin]);
        var otherCells = new HashSet<Vector3Int>(vertexMap[vOther]);
        //Look don't ask how you figured this out but just be glad you did

        //For jumps to be valid, exactly one cell must be common to the origin vertex

        originCells.IntersectWith(otherCells);
        return originCells.Count == 1;
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
