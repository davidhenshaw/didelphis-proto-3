using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePanelView : MonoBehaviour
{
    [SerializeField]
    TMP_Text valueText;

    private void Awake()
    {
        GameSession.ScoreChanged += UpdateView;
        valueText.text = "0";
    }

    public void UpdateView(ScorePanelModel model)
    {
        valueText.text = model.Score.ToString();
    }
}
