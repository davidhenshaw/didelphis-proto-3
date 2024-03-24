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
}

public interface IContainable
{
    GameObject Owner { get; }
    Vector3 AnchorLocalOffset { get; }
    Vector2Int[] GetCellRelativePositions();
}
