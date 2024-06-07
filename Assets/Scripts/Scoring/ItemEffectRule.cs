using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoring Rules/Item Effect Count")]
public class ItemEffectRule : ScoreRule<ItemContainer>
{
    [SerializeField]
    TileAttributes TileAttribute;

    public override float GetProgress(ItemContainer obj, int numEffects, bool invert = false)
    {
        int effectCount = 0;
        if(ItemTile.ATTRIBUTE_MAP.TryGetValue(TileAttribute, out Type effectType))
        {
            //Search the item container for items with this effect
            // and count them
            foreach(var item in obj.Cells.Values)
            {
                if(item.Owner.TryGetComponent(effectType, out Component component))
                    effectCount++;
            }
        }

        return (float)effectCount/ numEffects;
    }

    public override int GetScore(ItemContainer obj)
    {
        throw new System.NotImplementedException();
    }
}
