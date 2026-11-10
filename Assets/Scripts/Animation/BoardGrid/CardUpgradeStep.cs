using System;
using FFF7R;

sealed class CardUpgradeStep : IAnimStep
{
    readonly CardBoard _board;
    readonly Int2 _pos;
    readonly int _value;

    readonly Action _onBegin;
    GridAnim _anim;
    float _duration = 0;
    float _t;

    public CardUpgradeStep(CardBoard board, Int2 pos, int value, Action onBegin) {
        _board = board;
        _pos = pos;
        _onBegin = onBegin; 
        _value = value;
    }

    public void Begin()
    {
        _onBegin?.Invoke();

        _anim = _board[_pos].Anim;
        _anim.CardUpgradeVFX(_value);
        _duration = _board[_pos].Anim.CardUpgradeVfxDuration;
    }

    public bool Tick(float dt) { 
        _t += dt; 
        return _t >= _duration;
    }

    public void Finish()
    {
        _anim.StopCardUpgradeVFX();
    }
}