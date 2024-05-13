using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public TimerView View;

    public float StartTime = 10;

    private Timer _timer;
    private TimerModel _timerModel;

    private void Awake()
    {
        _timer = new Timer(Timer.TimerMode.COUNT_DOWN, StartTime, true);
        _timerModel = new TimerModel();
        _timer.SetTime(StartTime);
    }

    private void Update()
    {
        _timer.Tick(Time.deltaTime);
        _timerModel.timeInSeconds = _timer.GetCurrent();

        if(!_timer.Paused)
            View?.UpdateViewWithModel(_timerModel);
    }
}
