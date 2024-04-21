using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CrushableProperty : ItemProperty, IPropertyHandler, IGridMovementValidator
{
    private List<Vector2Int> _toRemove = new List<Vector2Int>();
    bool isCrushed = false;

    public Sprite CrushedSpriteVertical;
    public Sprite CrushedSpriteHorizontal;

    public override void Tick()
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
            CrushVertical(heavy.Item);
        }
    }

    private bool IsCellOwnedByItem(Item item, Vector2Int containerCell)
    {
        var container = item.Container;

        if (!container.Cells.TryGetValue(containerCell, out IGridContainable itemBelow))
            return false;

        return item.Equals(itemBelow);
    }

    public bool CanResolveCollision(IGridContainable[] items)
    {
        return items.All((item) => CanBeCrushed(item));
    }

    private bool CanBeCrushed(IGridContainable crusher)
    {
        if (isCrushed)
            return false;

        //Figure out which tiles to delete
        Vector2Int crusherAnchor = Item.Container.GetAnchorCell(crusher);
        Vector2Int itemAnchor = Item.Container.GetAnchorCell(Item);
        IGridContainer container = Item.Container;
        int rowToDelete = 100;
        foreach(var localCell in crusher.GetCellRelativePositions())
        {
            var containerCell = crusherAnchor + localCell;
            if (!container.Cells.TryGetValue(containerCell + Vector2Int.down, out IGridContainable itemBelow))
                continue;
            var isCellBelowSelf = crusher.Equals(itemBelow); 
            if (!isCellBelowSelf)
            {
                rowToDelete = Mathf.Min(rowToDelete, (containerCell + Vector2Int.down).y);
            }
        }

        foreach(var localCell in Item.GetCellRelativePositions() )
        {
            var containerCell = itemAnchor + localCell;
            var isInDeleteRow = containerCell.y == rowToDelete; 
            var isCrushable = IsCellOwnedByItem(Item, containerCell + Vector2Int.down);

            if (!isInDeleteRow)
                continue;

            // Cells are only crushable if there is at least one other cell under it
            if(isCrushable)
            {
                continue;
            }
            else
            {//if even one cell in the delete row isn't crushable, stop. Do not crush this item at all
                return false;
            }
        }

        return true;
    }

    public void CrushVertical(Item crusher)
    {
        if (isCrushed)
            return;
        //Figure out which tiles to delete
        Vector2Int crusherAnchor = Item.Container.GetAnchorCell(crusher);
        Vector2Int itemAnchor = Item.Container.GetAnchorCell(Item);
        IGridContainer container = Item.Container;
        int rowToDelete = 100;
        foreach(var localCell in crusher.GetCellRelativePositions())
        {
            var containerCell = crusherAnchor + localCell;
            if (!container.Cells.TryGetValue(containerCell + Vector2Int.down, out IGridContainable itemBelow))
                continue;
            var isCellBelowSelf = crusher.Equals(itemBelow); 
            if (!isCellBelowSelf)
            {
                rowToDelete = Mathf.Min(rowToDelete, (containerCell + Vector2Int.down).y);
            }
        }

        List<Vector2Int> crushedTiles = new List<Vector2Int>();
        foreach(var localCell in Item.GetCellRelativePositions() )
        {
            var containerCell = itemAnchor + localCell;
            var isInDeleteRow = containerCell.y == rowToDelete; 
            var isCrushable = IsCellOwnedByItem(Item, containerCell + Vector2Int.down);

            if (!isInDeleteRow)
                continue;

            // Cells are only crushable if there is at least one other cell under it
            if(isCrushable)
            {
                crushedTiles.Add(containerCell);
            }
            else
            {//if even one cell in the delete row isn't crushable, stop. Do not crush this item at all
                return;
            }
        }

        //Remove the local cells from the container controller
        _toRemove.AddRange(crushedTiles);

        //Remove local cells, locally
        foreach(var cellToRemove in crushedTiles)
        {
            Item.RemoveLocalCell(cellToRemove - itemAnchor);
        }

        Item.RecalculateAnchor();
        isCrushed = true;
        //Swap normal visuals of item with crushed visuals
        var itemSprite = GetComponentInChildren<SpriteRenderer>();

        itemSprite.sprite = (Item.Orientation == Orientation.Up || Item.Orientation == Orientation.Down) ? 
            CrushedSpriteVertical : CrushedSpriteHorizontal;

        //Assertions
        Assert.IsTrue(Item.GetCellRelativePositions().Contains(Vector2Int.zero), "Anchor cell was deleted");
    }
}
