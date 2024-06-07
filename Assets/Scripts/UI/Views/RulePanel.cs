using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulePanel : MonoBehaviour
{
    [SerializeField]
    GameObject widgetPrefab;
    Criteria[] _criteria;

    private List<RuleWidgetView> _widgets = new();
    private void Awake()
    {
        GameSession.GameSessionChanged += OnGameSessionInit;
        GameSession.ScoreChanged += UpdateViews;
    }

    void OnGameSessionInit(GameSession gameSession)
    {
        _criteria = gameSession.CurrentCriteria;
        _widgets.Clear();
        
        foreach(var criterion in _criteria) {
            var instance = Instantiate(widgetPrefab, transform).GetComponent<RuleWidgetView>();
            instance.Rule = criterion.Rule;

            _widgets.Add(instance); 
        }

    }

    public void UpdateViews(RulePanelModel model)
    {
        RuleWidgetModel widgetModel = new RuleWidgetModel();
        for(int i = 0; i < model.Criteria.Length; i++)
        {
            var widget = _widgets.ToArray()[i];
            widgetModel.Progress = model.Progress[i];

            widget.UpdateView(widgetModel);
        }
    }
}

[System.Serializable]
public struct RulePanelModel
{
    public int Score;
    public float[] Progress;
    public ItemContainer Container;
    public Criteria[] Criteria;
}

