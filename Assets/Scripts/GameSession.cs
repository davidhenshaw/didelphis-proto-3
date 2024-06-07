using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public int Score => _rulePanelModel.Score;
    private RulePanelModel _rulePanelModel;

    public static Action<RulePanelModel> ScoreChanged;
    public static Action<GameSession> GameSessionChanged;

    [SerializeField]
    private Criteria[] _criteria;
    public Criteria[] CurrentCriteria => _criteria;

    private void Awake()
    {
        GameSessionChanged?.Invoke(this);
    }
    private void Start()
    {
        ContainerController.ContainerItemsUpdated += OnContainerUpdated;
    }

    public void OnContainerUpdated(ItemContainer container)
    {
        StartCoroutine(nameof(NotifyScoreChange), container);
    }

    private IEnumerator NotifyScoreChange(ItemContainer container)
    {
        float[] progressAmounts = new float[_criteria.Length];
        for (int i = 0; i < _criteria.Length; i++)
        {
            var criterion = _criteria[i];
            progressAmounts[i] = criterion.Rule.GetProgress(container, criterion.number, criterion.invert);
        }

        _rulePanelModel.Container = container;
        _rulePanelModel.Progress = progressAmounts;
        _rulePanelModel.Criteria = CurrentCriteria;

        yield return null; //Wait a frame for components to be added/destoryed

        ScoreChanged?.Invoke(_rulePanelModel);
        //Debug.Log("Score: " + Score);
    }
}

[System.Serializable]
public class Criteria
{
    public ScoreRule<ItemContainer> Rule;
    public bool invert;
    public int number;
    public int id;
}
