using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public int Score => _scoreModel.Score;
    private ScorePanelModel _scoreModel;

    public static Action<ScorePanelModel> ScoreChanged;

    public List<ScoreRule<ItemContainer>> scoreRules;

    private void Awake()
    {
        _scoreModel = new ScorePanelModel();
    }
    private void Start()
    {
        ContainerController.ContainerClosed += TallyScore;
    }
    public void TallyScore(ItemContainer container)
    {
        foreach (var rule in scoreRules)
        {
            int value = rule.GetScore(container);
            _scoreModel.Score += value;

            Debug.Log($"{rule.name} returned {value}");
        }

        ScoreChanged?.Invoke(_scoreModel);
        Debug.Log("Score: " + Score);
    }
}
