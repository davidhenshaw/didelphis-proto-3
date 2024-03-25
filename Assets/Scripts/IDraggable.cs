using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    void OnDragStart();
    void OnDrop();
    void SetDragTarget(Transform transform);
}

public interface IContainer
{
    void OnDrop(IContainable containable);
    bool TryAddItem(IContainable item, Vector2Int insertPos);
    bool TryRemoveItem(IContainable item);
}

public interface IContainable
{
    GameObject Owner { get; }
    IContainer Container { get; set; }
    Vector3 AnchorLocalOffset { get; }
    Vector2Int[] GetCellRelativePositions();
}
