using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/ContainerTile")]
public class ContainerTile : Tile
{
    public enum AccessType
    {
        Allowed, Not_Allowed
    }

    public AccessType accessType;
}
