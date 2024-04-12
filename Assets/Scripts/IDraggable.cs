using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    void OnDragStart();
    void OnDrop();
    void SetDragTarget(Transform transform);
}

public interface IGridContainer
{
    Dictionary<Vector2Int, IGridContainable> Cells { get; }

    Vector2Int GetAnchorCell(IGridContainable item);
    void OnDrop(IGridContainable containable);
    void OnPick(IGridContainable containable);
    bool TryAddItem(IGridContainable item, Vector2Int insertPos);
    bool TryRemoveItem(IGridContainable item);
}

public interface IGridContainable
{
    GameObject Owner { get; }
    Orientation Orientation { get; }
    IGridContainer Container { get; set; }
    Vector3 AnchorLocalPosition { get; }
    Vector3 AnchorWorldPosition { get; }
    Vector2Int[] GetCellRelativePositions();
    void Rotate(Item.RotationType rotationType);

}

public enum Orientation
{
    Up = 0,
    Right = 1,
    Down = 2, 
    Left = 3,
}
