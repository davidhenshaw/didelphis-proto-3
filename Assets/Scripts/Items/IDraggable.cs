using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IDraggable
{
    delegate void DragEvent(Transform target, Vector3 offset);
    public event DragEvent DragStarted;
    public event DragEvent DragFinished;
    void OnDragStart(Transform target);
    void OnDrop();
    void OnDrag();

}

public interface IRotate
{
    void Rotate(Item.RotationType rotationType);
}

public interface IBucket
{
    void OnDrop(IGridContainable containable);
    void OnHover(IGridContainable containable);
    void OnHoverEnd();
}

public interface IGridContainer
{
    Dictionary<Vector2Int, IGridContainable> Cells { get; }

    Vector2Int GetAnchorCell(IGridContainable item);
    //void OnDrop(IGridContainable containable);
    void OnPick(IGridContainable containable);
    //void OnHover(IGridContainable containable);
    bool TryAddItem(IGridContainable item, Vector2Int insertPos);
    bool TryRemoveItem(IGridContainable item);
    //void OnHoverEnd();
}

public interface IGridContainable
{
    GameObject Owner { get; }
    Orientation Orientation { get; }
    IGridContainer Container { get; set; }
    Vector2Int LocalGridAnchor { get; }
    Vector3 AnchorLocalPosition { get; }
    Vector3 AnchorWorldPosition { get; }
    Vector2Int[] GetCellRelativePositions();
    Vector2Int[] BorderPositions { get; }
    Tilemap GetLayoutTilemap();
    void Rotate(Item.RotationType rotationType);

}

public enum Orientation
{
    Up = 0,
    Right = 1,
    Down = 2, 
    Left = 3,
}
