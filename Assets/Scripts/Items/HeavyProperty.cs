using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyProperty : ItemProperty, IGridMovementValidator
{
    public bool CanResolveCollision(IGridContainable[] items)
    {
        HashSet<IGridContainable> itemsBelow = new HashSet<IGridContainable>();
        HashSet<IGridContainable> collisionItems = new HashSet<IGridContainable>();
        collisionItems.AddRange(items);
        itemsBelow.AddRange(GetItemsBelow());

        return itemsBelow.SetEquals(collisionItems);
    }

    public override void Tick()
    {
        //var itemsBelow = GetItemsBelow();

        //foreach(var item in itemsBelow)
        //{
        //    if(item.Owner.TryGetComponent(out IPropertyHandler itemHandler))
        //    {
        //        itemHandler.ProcessProperty(this);
        //    }
        //}
    }

    private IGridContainable[] GetItemsBelow()
    {
        List<IGridContainable> items = new List<IGridContainable>();
        //Get my item's current container
        if (_Item?.Container == null)
            return null;

        //Search for items underneath the current item and interact with them
        var anchorAbsolutePos = _Item.Container.GetAnchorCell(_Item);
        foreach(var cellOffset in _Item.GetCellRelativePositions())
        {
            if(!_Item.Container.Cells.TryGetValue(cellOffset + anchorAbsolutePos + Vector2Int.down, out IGridContainable item))
                continue;

            items.Add(item);
        }

        return items.ToArray();
    }
}
