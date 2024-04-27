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

        var positions = GetVerticesFromCells(model.highlightedCells);

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }

    private Vector3[] GetVerticesFromCells(Vector3Int[] cells)
    {
        Dictionary<Vector3, int> vertexRepetitions = new Dictionary<Vector3, int>();
        SortedSet<Vector3> perimeter = new SortedSet<Vector3>();
        Vector3 avgPosition = Vector3.zero;

        void CountVertex(Vector3 vertex)
        {
            if(vertexRepetitions.ContainsKey(vertex))
            {
                vertexRepetitions[vertex]++;
            }
            else
            {
                vertexRepetitions.Add(vertex, 1);
            }
        }

        foreach(var cell in cells)
        {
            float xOffset = _grid.cellSize.x * 0.5f;
            float yOffset = _grid.cellSize.y * 0.5f;
            var pointNW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset, 0);
            var pointNE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset, 0);
            var pointSW = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset * -1, yOffset * -1, 0);
            var pointSE = _grid.GetCellCenterLocal(cell) + new Vector3(xOffset, yOffset * -1, 0);


            CountVertex(pointNW);
            CountVertex(pointNE);
            CountVertex(pointSE);
            CountVertex(pointSW);

            avgPosition += pointNE + pointNW + pointSE + pointSW;
        }

        //Points on the perimeter will only repeat less than 4 times. If the same vertex is counted 4 times, it is internal to the shape
        //Get average position of PERIMETER points only.
        avgPosition /= vertexRepetitions.Count((kvp) => {return kvp.Value < 4; });

        ClockwiseComparer comparer = new ClockwiseComparer(avgPosition);
        perimeter = new SortedSet<Vector3>(comparer);


        foreach(var vertex in vertexRepetitions.Keys)
        {
            if (vertexRepetitions[vertex] >= 4)
                continue;
            perimeter.Add(vertex);
        }

        return perimeter.ToArray();
    }

    private class ClockwiseComparer : IComparer<Vector3>
    {
        public Vector3 pivot;

        public ClockwiseComparer(Vector3 averagePoint)
        {
            this.pivot = averagePoint;
            pivot.z = 0;
        }

        public int Compare(Vector3 v1, Vector3 v2)
        {
            v1.z = 0;
            v2.z = 0;
            //Get the vector between each point and the average position
            var v1pivot = v1 - pivot;
            var v2pivot = v2 - pivot;

            var v1angle = Vector3.SignedAngle(v1pivot, Vector3.down, Vector3.forward);
            var v2angle = Vector3.SignedAngle(v2pivot, Vector3.down, Vector3.forward);
            return Mathf.CeilToInt(v1angle - v2angle);
        }
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
