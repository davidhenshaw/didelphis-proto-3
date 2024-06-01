using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public int Score => _rulePanelModel.Score;
    private RulePanelModel _rulePanelModel;

    public static Action<RulePanelModel> ScoreChanged;

    public List<ScoreRule<ItemContainer>> Rules;

    private void Awake()
    {
    }
    private void Start()
    {
        ContainerController.ContainerClosed += TallyScore;
        ContainerController.ContainerItemsUpdated += OnContainerUpdated;
    }

    public void OnContainerUpdated(ItemContainer container)
    {
        StartCoroutine(nameof(NotifyScoreChange), container);
    }

    private IEnumerator NotifyScoreChange(ItemContainer container)
    {
        foreach (var rule in Rules)
        {
            float value = rule.GetProgress(container);

            Debug.Log($"{rule.name} returned {value}");
        }

        _rulePanelModel.Container = container;

        yield return null; //Wait a frame for components to be added/destoryed

        ScoreChanged?.Invoke(_rulePanelModel);
        Debug.Log("Score: " + Score);
    }

    public void TallyScore(ItemContainer container)
    {
        foreach (var rule in Rules)
        {
            float value = rule.GetProgress(container);

            Debug.Log($"{rule.name} returned {value}");
        }

        ScoreChanged?.Invoke(_rulePanelModel);
        Debug.Log("Score: " + Score);
    }
}
