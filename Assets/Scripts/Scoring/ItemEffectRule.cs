using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoring Rules/Item Effect Count")]
public class ItemEffectRule : ScoreRule<ItemContainer>
{
    [SerializeField]
    TileAttributes TileAttribute;

    public override float GetProgress(ItemContainer obj, int goal, bool invert = false)
    {
        int effectCount = 0;

        HashSet<IGridContainable> containerItems = new HashSet<IGridContainable>();
        containerItems.AddRange(obj.Cells.Values); //Eliminates duplicates

        if(ItemTile.ATTRIBUTE_MAP.TryGetValue(TileAttribute, out Type effectType))
        {
            //Search the item container for items with this effect
            // and count them
            foreach(var item in containerItems)
            {
                if(item.Owner.TryGetComponent(effectType, out Component component))
                    effectCount++;
            }
        }

        if (invert)
        {
            effectCount = containerItems.Count - effectCount;
        }

        return (float)effectCount/ goal;
    }

    public override int GetScore(ItemContainer obj)
    {
        throw new System.NotImplementedException();
    }
}
