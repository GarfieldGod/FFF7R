using System;
using System.Collections.Generic;
using FFF7R;
using UnityEngine;

sealed class CardPlacedStep : IAnimStep
{
    readonly BoardGrid _grid;
    readonly string _card;
    readonly PlayerSeat _owner;

    readonly Action _onBegin;
    float _duration = 0;
    float _t;

    public CardPlacedStep(BoardGrid grid, string card, PlayerSeat owner, Action onBegin) {
        _grid = grid;
        _card = card;
        _onBegin = onBegin;
        _owner = owner;
    }

    public void Begin()
    {
        _grid.Anim.CardPlaced(_card);
        // _duration =  _grid.Anim.Duration;

        _onBegin?.Invoke();
    }

    public bool Tick(float dt) { 
        _t += dt; 
        return _t >= _duration;
    }

    public void Finish()
    {
        _grid?.Anim.Stop();
    }
}