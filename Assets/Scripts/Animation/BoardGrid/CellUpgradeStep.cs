using System;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

sealed class CellUpgradeStep : IAnimStep
{
    readonly Dictionary<BoardGrid, CellDelta> _ap;
    List<GridAnim> _pendingAnim = new List<GridAnim>();
    readonly Action _onBegin;
    float _duration = 0;
    float _t;

    public CellUpgradeStep(Dictionary<BoardGrid, CellDelta> ap, Action onBegin) { _ap = ap; _onBegin = onBegin; }

    public void Begin()
    {
        foreach (var p in _ap)
        {
            var cell = p.Key;
            _pendingAnim.Add(cell.Anim);
            cell.Anim.CellUpgrade(p.Value);
            _duration = Mathf.Max(_duration, cell.Anim.Duration);
        }

        _onBegin?.Invoke();
    }

    public bool Tick(float dt) { 
        _t += dt; 
        return _t >= _duration;
    }

    public void Finish()
    {
        // 保险：跳到终态
        foreach(var anim in _pendingAnim)
        {
            anim.Stop();
        }
    }
}