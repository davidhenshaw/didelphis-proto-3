using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/ItemTile")]
public class ItemTile : Tile
{
    public static readonly Dictionary<TileAttributes, Type> ATTRIBUTE_MAP = new Dictionary<TileAttributes, Type>()
    {
        {TileAttributes.Frozen, typeof(FrozenItemEffect) }
    };

    [SerializeField]
    private TileAttributes m_Attribute;
    public TileAttributes Attributes { get { return m_Attribute; } }
    public Type ItemEffectType { get {
            if (m_Attribute == TileAttributes.None)
                return null;
            else
                return ATTRIBUTE_MAP[m_Attribute];
        } }

    public TileAttributes Attribute { get => m_Attribute; private set => m_Attribute = value; }
    
    public void ApplyItemEffect(GameObject target)
    {
        if(m_Attribute == TileAttributes.None) return;

        if (target.TryGetComponent(ATTRIBUTE_MAP[m_Attribute], out var effect))
            return;

        target.AddComponent(ATTRIBUTE_MAP[m_Attribute]);
    }

    public bool IsTypeMatch<T>(T tileEffectType) where T : Type
    {
        if (m_Attribute == TileAttributes.None) return false;
        return ATTRIBUTE_MAP[m_Attribute].Equals(tileEffectType);
    }
}

public enum TileAttributes
{
    None, Frozen
}
