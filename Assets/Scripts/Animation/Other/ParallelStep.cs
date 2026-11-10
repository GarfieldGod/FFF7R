using System.Collections.Generic;

sealed class ParallelStep : IAnimStep
{
    readonly List<IAnimStep> _steps;
    public ParallelStep(List<IAnimStep> steps) { _steps = steps; }
    public void Begin()  { for (int i = 0; i < _steps.Count; i++) _steps[i].Begin(); }
    public bool Tick(float dt)
    {
        bool done = true;
        for (int i = 0; i < _steps.Count; i++) done &= _steps[i].Tick(dt);
        return done;
    }
    public void Finish() { for (int i = 0; i < _steps.Count; i++) _steps[i].Finish(); }
}