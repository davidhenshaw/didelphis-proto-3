using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyProperty : ItemProperty
{
    public override void Invoke()
    {
        //Get my item's current container
        if (_Item?.Container == null)
            return;

        //Search for items underneath the current item and interact with them
        var anchorAbsolutePos = _Item.Container.GetAnchorCell(_Item);
        foreach(var cellOffset in _Item.GetCellRelativePositions())
        {
            if(!_Item.Container.Cells.TryGetValue(cellOffset + anchorAbsolutePos + Vector2Int.down, out IGridContainable item))
                continue;

            if(item.Owner.TryGetComponent(out IPropertyHandler itemHandler))
            {
                itemHandler.ProcessProperty(this);
            }
        }
    }
}
