using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemTile : Tile
{
    public static readonly Dictionary<TileAttributes, Type> ATTRIBUTE_MAP = new Dictionary<TileAttributes, Type>() 
    {
        {TileAttributes.Frozen, typeof(FrozenItemEffect) }
    };

    [SerializeField]
    private TileAttributes m_Attribute;

    public TileAttributes Attribute { get => m_Attribute; private set => m_Attribute = value; }
    
    public void ApplyItemEffect(GameObject target)
    {
        if (target.TryGetComponent(ATTRIBUTE_MAP[m_Attribute], out var effect))
            return;

        target.AddComponent(ATTRIBUTE_MAP[m_Attribute]);
    }
}

public enum TileAttributes
{
    None, Frozen
}
