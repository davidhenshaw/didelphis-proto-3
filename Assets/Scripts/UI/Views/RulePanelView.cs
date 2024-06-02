using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulePanelView : MonoBehaviour
{
    [SerializeField]
    GameObject widgetPrefab;
    ScoreRule<ItemContainer>[] _rules;

    private List<RuleWidgetView> _widgets = new();
    private void Awake()
    {
        GameSession.GameSessionChanged += OnGameSessionInit;
        GameSession.ScoreChanged += UpdateView;
    }

    void OnGameSessionInit(GameSession gameSession)
    {
        _rules = gameSession.Rules.ToArray();        
        _widgets.Clear();
        
        foreach(var rule in _rules) {
            var instance = Instantiate(widgetPrefab, transform).GetComponent<RuleWidgetView>();
            instance.Rule = rule;

            _widgets.Add(instance); 
        }

    }

    public void UpdateView(RulePanelModel model)
    {
        RuleWidgetModel widgetModel = new RuleWidgetModel();
        foreach(var widget in _widgets)
        {
            widgetModel.Progress = widget.Rule.GetProgress(model.Container);

            widget.UpdateView(widgetModel);
        }
    }
}

[System.Serializable]
public struct RulePanelModel
{
    public int Score;
    public float Progress;
    public ItemContainer Container;
}

