using System;

sealed class ActionStep : IAnimStep
{
    Action _action;
    float _delay;
    float _t;
    public ActionStep(Action action, float delay = 0)
    {
        this._action = action;
        this._delay = delay;
    }

    public void Begin() {}
    public bool Tick(float dt)
    {
        _t += dt; 
        return _t >= _delay;
    }
    public void Finish() { _action?.Invoke(); }
}