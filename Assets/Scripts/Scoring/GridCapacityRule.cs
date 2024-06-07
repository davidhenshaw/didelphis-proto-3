using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoring Rules/Grid Capacity")]
public class GridCapacityRule : ScoreRule<ItemContainer>
{
    [SerializeField] 
    private int _baseScore = 80;

    [SerializeField]
    private int _fullBonus = 50;

    [SerializeField]
    [Range(-100, 0)]
    private int _emptySpacePenalty = -10;

    public override float GetProgress(ItemContainer obj, int num, bool invert = false)
    {
        throw new System.NotImplementedException();
    }

    public override int GetScore(ItemContainer obj)
    {
        var emptySpaces = obj.CellCapacity - obj.Cells.Count;
        var bonus = emptySpaces == 0 ? _fullBonus : 0;
        return _emptySpacePenalty * emptySpaces + _baseScore + bonus;
    }
}
