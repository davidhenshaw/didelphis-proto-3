using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemTile : Tile
{
    [SerializeField]
    private ItemAttribute m_Attribute;

    public ItemAttribute Attribute { get => m_Attribute; private set => m_Attribute = value; }
}

public enum TileAttributes
{
    None, Crushable_Vertical, Crushable_Horizontal
}
