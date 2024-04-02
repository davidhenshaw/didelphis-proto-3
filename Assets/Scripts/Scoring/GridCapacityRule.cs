using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoring Rules/Grid Capacity")]
public class GridCapacityRule : ScoreRule<ItemContainer>
{
    [SerializeField] 
    private int _baseScore = 80;

    [SerializeField]
    [Range(-100, 0)]
    private int _emptySpacePenalty = -10;

    public override int GetScore(ItemContainer obj)
    {
        var emptySpaces = obj.CellCapacity - obj.Cells.Count;
        return _emptySpacePenalty * emptySpaces + _baseScore;
    }
}
