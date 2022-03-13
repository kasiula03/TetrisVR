using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimersManager : MonoBehaviour
{
    private readonly List<TimerAction> _timers = new List<TimerAction>();

    public void Subscribe(TimerAction action, bool invokeOnSub)
    {
        if (invokeOnSub)
        {
            action.Invoke();
        }
        else
        {
            action.SetupCurrentTime();
        }

        _timers.Add(action);
    }

    public void Unsubscribe(TimerAction action)
    {
        if (_timers.Contains(action))
        {
            action.OnStop();
            _timers.Remove(action);
        }
    }

    private void Update()
    {
        foreach (TimerAction timerAction in _timers.ToList())
        {
            if (timerAction.LastTimeInvoke + timerAction.Duration < Time.time)
            {
                timerAction.Invoke();
            }
        }
    }
}