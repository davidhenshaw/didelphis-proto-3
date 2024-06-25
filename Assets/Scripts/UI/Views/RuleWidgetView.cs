using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuleWidgetView : MonoBehaviour
{
    public Sprite completeSprite;
    public Sprite incompleteSprite;

    public TMP_Text labelText;
    public TMP_Text description;
    public Image image;

    public ScoreRule<ItemContainer> Rule;

    public void UpdateView(RuleWidgetModel model)
    {
        image.sprite = model.Progress >= 0.99f ? completeSprite : incompleteSprite;
        labelText.text = model.RuleName;
        description.text = model.RuleDescription;
    }
}


public struct RuleWidgetModel
{
    public string RuleName;
    public string RuleDescription;
    public float Progress;
}
