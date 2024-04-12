using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CrushableProperty : ItemProperty, IPropertyHandler
{
    private List<Vector2Int> _toRemove = new List<Vector2Int>();
    bool isCrushed = false;

    public Sprite CrushedSpriteVertical;
    public Sprite CrushedSpriteHorizontal;

    public override void Invoke()
    {
    }

    public override Vector2Int[] GetCellsToRemove()
    {
        var ret = _toRemove.ToArray();
        _toRemove.Clear();
        return ret;
    }
    public void ProcessProperty(ItemProperty property)
    {
        if(property.TryGetComponent(out HeavyProperty heavy))
        {//crush this object
            CrushItemVertical(heavy.Item);
        }
    }

    public void CrushItemVertical(Item crusher)
    {
        if (isCrushed)
            return;
        //Figure out which tiles to delete
        Vector2Int crusherAnchor = Item.Container.GetAnchorCell(crusher);
        Vector2Int itemAnchor = Item.Container.GetAnchorCell(Item);
        IGridContainer container = Item.Container;
        int rowToDelete = 0;
        foreach(var localCell in crusher.GetCellRelativePositions())
        {
            var absoluteCell = crusherAnchor + localCell;
            if (!container.Cells.TryGetValue(absoluteCell + Vector2Int.down, out IGridContainable itemBelow))
                continue;
            var isCellBelowSelf = crusher.Equals(itemBelow); 
            if (!isCellBelowSelf)
            {
                rowToDelete = (absoluteCell + Vector2Int.down).y;
            }
        }

        List<Vector2Int> crushedTiles = new List<Vector2Int>();
        foreach(var localCell in Item.GetCellRelativePositions() )
        {
            var absoluteCell = itemAnchor + localCell;
            if(absoluteCell.y == rowToDelete)
            {
                crushedTiles.Add(absoluteCell);
            }
        }

        //Remove the local cells from the container controller
        _toRemove.AddRange(crushedTiles);

        //Remove local cells, locally
        foreach(var cellToRemove in crushedTiles)
        {
            Item.RemoveLocalCell(cellToRemove - itemAnchor);
        }

        isCrushed = true;
        //Swap normal visuals of item with crushed visuals
        var itemSprite = GetComponentInChildren<SpriteRenderer>();

        itemSprite.sprite = (Item.Orientation == Orientation.Up || Item.Orientation == Orientation.Down) ? 
            CrushedSpriteVertical : CrushedSpriteHorizontal;

        //Assertions
        Assert.IsTrue(Item.GetCellRelativePositions().Contains(Vector2Int.zero), "Anchor cell was deleted");
    }
}
