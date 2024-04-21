using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridContainerMovement : MonoBehaviour
{
    public ItemContainer Container;

    private Dictionary<IGridContainable, Vector2Int> _desiredMovements = new Dictionary<IGridContainable, Vector2Int>();
    private Dictionary<IGridContainable, MovementResult> _movementInfo = new Dictionary<IGridContainable, MovementResult>();

    public void RegisterMove(IGridContainable item, Vector2Int movement)
    {
        _desiredMovements.Add(item, movement);
    }

    public void DoMoves()
    {
        ExecuteMovements(CalculateExecutionOrder());
        _desiredMovements.Clear();
        _movementInfo.Clear();
    }

    private void ExecuteMovements(LinkedList<IGridContainable> orderedMovements)
    {
        LinkedListNode<IGridContainable> currItem = orderedMovements.First;

        while (currItem != null)
        {
            //Remove Item and reinsert in desired cell
            var anchor = Container.GetAnchorCell(currItem.Value);
            Container.TryRemoveItem(currItem.Value);

            var moved = Container.TryAddItem(currItem.Value, anchor + _desiredMovements[currItem.Value]);

            if (moved)
                Container.SnapToCell(currItem.Value, anchor + _desiredMovements[currItem.Value]);
            else
                Container.TryAddItem(currItem.Value, anchor);

            currItem = currItem.Next;
        }
    }

    private LinkedList<IGridContainable> CalculateExecutionOrder()
    {
        LinkedList<IGridContainable> _dependencyChain = new LinkedList<IGridContainable>();

        foreach(IGridContainable item in _desiredMovements.Keys)
        {
            if (_dependencyChain.Contains(item))
                continue;

            var itemAnchor = Container.GetAnchorCell(item);

            //if (!Container.TryRemoveItem(item))
            //{//If this item was never in the container....uh...idk how that would happen
            //    Debug.LogWarning("I can't believe you've done this");
            //    continue;
            //}

            MovementResult result = Container.CanMoveItem(item, itemAnchor + _desiredMovements[item]);
            //Container.TryAddItem(item, itemAnchor);

            if(result.CanMove == false || result.collisions == null)
            {
                _dependencyChain.AddLast(item);
                continue;
            }

            foreach(IGridContainable blocker in result.collisions)
            {
                if(_dependencyChain.Contains(blocker))
                    continue;
                _dependencyChain.AddLast(blocker);
            }
            _dependencyChain.AddLast(item);
            _movementInfo.Add(item, result);
        }

        return _dependencyChain;
    } 
}

public interface IGridMovementValidator
{
    bool CanResolveCollision(IGridContainable[] item);
}
