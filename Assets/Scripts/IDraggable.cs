using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    void ClearTarget();
    void SetDragTarget(Transform transform);
}
