using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/CriteriaTile")]
public class CriteriaTile : ContainerTile
{
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        accessType = AccessType.Allowed;
        return base.StartUp(position, tilemap, go);
    }
}
