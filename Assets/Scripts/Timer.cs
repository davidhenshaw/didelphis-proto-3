/// <summary>
/// Timers must be managed by a TimerManager. DO NOT create them without a TimerManager. Timers start paused by default
/// </summary>
public class Timer
{
    public event System.Action TimeExpired;

    public bool Paused = true;
    /// <summary>
    /// Set to true when the timer is unpaused and counts down to zero
    /// </summary>
    public bool IsDone
    {
        get;
        private set;
    }
    public TimerMode _mode;

    private float _currTime;
    private float _startTime;

    public Timer(TimerMode mode)
    {
        _mode = mode;
    }

    public Timer(TimerMode mode, float startTime) : this(mode)
    {
        _startTime = startTime;
    }

    public Timer(TimerMode mode, bool playOnAwake) : this(mode)
    {
        if (playOnAwake)
            Unpause();
    }

    public Timer(TimerMode mode, float startTime, bool playOnAwake) : this(mode, startTime)
    {
        if (playOnAwake)
            Unpause();
    }

    public void ChangeMode(TimerMode mode)
    {
        _mode = mode;
    }

    public float GetElapsed()
    {
        float elapsed = float.NaN;

        switch (_mode)
        {
            case TimerMode.COUNT_DOWN:
                elapsed = _startTime - _currTime;
                break;
            case TimerMode.COUNT_UP:
                elapsed = _currTime;
                break;
        }

        return elapsed;
    }

    public float GetCurrent()
    {
        return _currTime;
    }

    public void Start()
    {
        Paused = false;
    }

    /// <summary>
    /// Resets the timer to its initial time but unpauses
    /// </summary>
    public void Restart()
    {
        Reset();
        Paused = false;
    }

    /// <summary>
    /// Resets the timer to its initial time and pauses
    /// </summary>
    public void Reset()
    {
        switch (_mode)
        {
            case TimerMode.COUNT_DOWN:
                SetTime(_startTime);
                break;

            case TimerMode.COUNT_UP:
                _currTime = 0;
                break;
        }

        Paused = true;
    }

    public void SetTime(float time)
    {
        _currTime = time;

        if (time > 0)
            IsDone = false;
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Unpause()
    {
        Paused = false;
    }

    public void Tick(float deltaTime)
    {
        if (Paused)
            return;

        switch (_mode)
        {
            case TimerMode.COUNT_UP:
                _currTime += deltaTime;
                break;
            case TimerMode.COUNT_DOWN:
                _currTime -= deltaTime;
                break;
        }

        if (_currTime <= 0)
        {
            TimeExpired?.Invoke();
            IsDone = true;
            Pause();
            _currTime = 0;
        }
    }

    public enum TimerMode
    {
        COUNT_DOWN, COUNT_UP
    }
}