using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct TimerModel
{
    public float timeInSeconds;
}

public class TimerView : MonoBehaviour, IView<TimerModel>
{
    public TMP_Text timeText;

    public void UpdateViewWithModel(TimerModel model)
    {
        TimeSpan timespan = TimeSpan.FromSeconds(model.timeInSeconds);
        timeText.text = timespan.ToString("g");
    }
}
