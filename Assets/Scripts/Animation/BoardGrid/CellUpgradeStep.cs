using System;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

sealed class CellUpgradeStep : IAnimStep
{
    readonly CardBoard _board;
    readonly IReadOnlyList<CellDelta> _deltas;
    List<GridAnim> _pendingAnim = new List<GridAnim>();
    readonly Action _onBegin;
    float _duration = 0;
    float _t;

    public CellUpgradeStep(CardBoard board, IReadOnlyList<CellDelta> deltas, Action onBegin) {
        _board = board;
        _deltas = deltas;
        _onBegin = onBegin; 
    }

    public void Begin()
    {
        _onBegin?.Invoke();

        foreach (var d in _deltas)
        {
            var cell = _board[d.Pos];
            _pendingAnim.Add(cell.Anim);
            cell.Anim.CellUpgradeVFX(d);
            _duration = Mathf.Max(_duration, cell.Anim.CellUpgradeDuration);
        }
    }

    public bool Tick(float dt) { 
        _t += dt; 
        return _t >= _duration;
    }

    public void Finish()
    {
        foreach(var anim in _pendingAnim)
        {
            anim.StopCellUpgradeVFX();
        }
    }
}