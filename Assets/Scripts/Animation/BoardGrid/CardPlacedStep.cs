using System;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

sealed class CardPlacedStep : IAnimStep
{
    readonly CardBoard _board;
    readonly Int2 _pos;
    readonly string _card;
    readonly PlayerSeat _owner;

    readonly Action _onBegin;
    GridAnim _anim;
    float _t;
    bool _vfxPlayed = false;

    public CardPlacedStep(CardBoard board, Int2 pos, string card, PlayerSeat owner, Action onBegin) {
        _card = card;
        _onBegin = onBegin;
        _owner = owner;
        _board = board;
        _pos = pos;
    }

    public void Begin()
    {
        _onBegin?.Invoke();
        _anim = _board[_pos].Anim;
    }

    public bool Tick(float dt)
    {
        if (_board == null) return true;
        _t += dt;
        _anim.CardPlaceFlight(_t);
        if (!_vfxPlayed && _t >= _anim.FlightDuration) { _vfxPlayed = true; _anim.CardPlaceVFX(); }
        return _t >= _anim.FlightDuration + _anim.PlaceVfxDuration * 0.01f;
    }

    public void Finish()
    {
        _anim?.StopCardPlaceFlight();
        _anim?.StopCardPlaceVFX();
    }
}