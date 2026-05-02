using System;
using System.Collections.Generic;

public class TimedSequenceBuilder
{
    private float _interval = 1.0f;
    private int _startFrame = 0;
    private int _endFrame = 100;
    private Action<int> _onStepCallback;

    /// <summary>
    /// Defines the frequency of the sequence in seconds.
    /// </summary>
    public TimedSequenceBuilder Every(float seconds)
    {
        _interval = seconds;
        return this;
    }

    /// <summary>
    /// Sets the boundaries for a looping temporal sequence.
    /// </summary>
    public TimedSequenceBuilder LoopBetween(int start, int end)
    {
        _startFrame = start;
        _endFrame = end;
        return this;
    }

    /// <summary>
    /// Assigns the logic to be executed at each step of the sequence.
    /// </summary>
    public TimedSequenceBuilder OnStep(Action<int> stepAction)
    {
        _onStepCallback = stepAction;
        return this;
    }

    /// <summary>
    /// Finalizes the configuration. 
    /// In a refactored state, this might return a specialized FSM instance.
    /// </summary>
    public object Build()
    {
        // For now, this returns a configuration object or handles the registration 
        // with your Processing Groups.
        return new
        {
            Interval = _interval,
            Range = (_startFrame, _endFrame),
            Action = _onStepCallback
        };
    }
}