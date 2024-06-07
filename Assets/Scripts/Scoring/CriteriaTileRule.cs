using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoring Rules/Criteria Tiles Rule")]
public class CriteriaTilesRule : ScoreRule<ItemContainer>
{
    public override float GetProgress(ItemContainer container, int goalCount, bool invert = false)
    {
        //Get total number of criteria tiles
        container.TileMap.CompressBounds();
        BoundsInt tilemapBounds = container.TileMap.cellBounds;
        List<Vector3Int> criteriaPositions = new List<Vector3Int>();

        foreach(var cell in tilemapBounds.allPositionsWithin)
        {
            if(container.TileMap.GetTile<CriteriaTile>(cell))
                criteriaPositions.Add(cell);
        }

        int totalCriteria = criteriaPositions.Count;

        //Query item tile map to determine overlap with criteria tiles
        foreach(var criteriaCell in new List<Vector3Int>(criteriaPositions))
        {
            if(container.ItemTileMap.GetTile<ItemTile>(criteriaCell))
                criteriaPositions.Remove(criteriaCell);
        }

        return (float)(totalCriteria - criteriaPositions.Count) / (totalCriteria);
    }

    public override int GetScore(ItemContainer obj)
    {
        throw new System.NotImplementedException();
    }
}
