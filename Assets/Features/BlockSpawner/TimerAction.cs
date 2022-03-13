using System;
using UnityEngine;

public class TimerAction
{
    public readonly float Duration;
    public float LastTimeInvoke { get; private set; }

    private readonly Action _onTimeAction;
    private readonly Action _onTimerStopAction;

    public TimerAction(float duration, Action onTimeAction, Action onTimerStopAction = null)
    {
        Duration = duration;
        _onTimeAction = onTimeAction;
        _onTimerStopAction = onTimerStopAction;
    }

    public void OnStop()
    {
        _onTimerStopAction?.Invoke();
    }

    public void Invoke()
    {
        _onTimeAction?.Invoke();
        LastTimeInvoke = Time.time;
    }

    public void SetupCurrentTime()
    {
        LastTimeInvoke = Time.time;
    }
}