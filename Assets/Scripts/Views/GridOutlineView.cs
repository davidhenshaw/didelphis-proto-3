using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridOutlineView : GridHighlightView
{
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [HideInInspector]
    [SerializeField]
    private GridHighlightModel _model;

    [SerializeField]
    private Color _validColor = Color.green;
    [SerializeField]
    private Color _invalidColor = Color.red;

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
        UpdateViewWithModel(_model);
    }

    public override void UpdateViewWithModel(GridHighlightModel model)
    {
        if (!_grid.Equals(model.grid))
            return;

        if (model.highlightedCells?.Length <= 0)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        var positions = GetPerimeterVertices(model.highlightedCells);
        if (positions == null)
            return;

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
        _lineRenderer.startColor = model.isValid ? _validColor : _invalidColor;
        _lineRenderer.endColor = model.isValid ? _validColor : _invalidColor;
    }

    /// <summary>
    /// Given an array of <paramref name="cells"/>, returns the perimeter vertices in an order where a line can be drawn through them. <br/>
    /// Returns null if the cells are not contiguous
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private Vector3[] GetPerimeterVertices(Vector3Int[] cellCollection)
    {
        List<Vector3Int> cells = new List<Vector3Int>(cellCollection);

        if (!IsContiguous(cells))
        {
            Debug.LogWarning("Group of cells is not contiguous and will not be rendered.");
            return null;
        }

        //if this shape has holes in it, fill them in so that we draw around them
        if(HasHoles(cells, out Vector3Int[] holes))
        {
            foreach( var hole in holes )
                cells.Add(hole);
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

        //Generate vertices for each cell
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

            //starting vertex for traversal is the most northwestern point we can find on the shape
            startVertex = (pointNW.x <= startVertex.x) && (pointNW.y >= startVertex.y) ? pointNW : startVertex;
        }

        startVertex = vertexMap.First().Key;
        
        List<Vector3> orderedList = new();
        List<Vector3> remainingVertices = new List<Vector3>(vertexMap.Keys);
        Vector3 currVertex = startVertex;
        orderedList.Add(startVertex);
        remainingVertices.Remove(startVertex);

        //Traverse vertices in the order that they will be connected
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
            var nextCandidate = nextChecks[i];
            if (vertices.Contains(nextCandidate))
            { //Validate jump between vertices
                if (!IsJumpValid(currVertex, nextCandidate, vertexMap))
                    continue;

                nextVertex = nextChecks[i];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// For jumps to be valid:<br/>
    /// <list type="bullet">
    /// <item><strong>exactly one cell</strong> must be shared between the <paramref name="vOrigin"/> and <paramref name="vOther"/> vertex</item>
    /// <item>The cells that share the vertices <paramref name="vOrigin"/> and <paramref name="vOther"/> must be contiguous</item>
    /// </list>
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

        var sharedCellsPlusOrigin = new HashSet<Vector3Int>(originCells);
        sharedCellsPlusOrigin.IntersectWith(otherCells);
        sharedCellsPlusOrigin.UnionWith(originCells);

        //Each of the cells that share the vertices vOrigin and vOther must be contiguous with vOrigin's cells
        if (!IsContiguous(sharedCellsPlusOrigin))
            return false;

        //For jumps to be valid, exactly one cell must be common to the origin vertex
        originCells.IntersectWith(otherCells);
        if(originCells.Count != 1)
            return false;

        
        return originCells.Count == 1;
    }
}
