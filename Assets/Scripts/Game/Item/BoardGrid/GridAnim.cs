
using System;
using FFF7R;
using FFF7R.VFX;
using UnityEngine;

[Serializable]
public class GridAnim
{
    public float Duration = 0;
    [SerializeField] private BoardCellUpgradeGroup _cellUpgrade;
    [SerializeField] private BoardCellUpgradeGroup _cardPlaced;

    public void CellUpgrade(CellDelta delta)
    {
        _cellUpgrade.colorTint = delta.Owner == PlayerSeat.Player ? Color.white : Color.red;

        _cellUpgrade.Play(delta.After);
        Duration = _cellUpgrade.TotalDuration;
    }

    public void CardPlaced(string cardCode)
    {
        _cardPlaced.Play();
        Duration = _cardPlaced.TotalDuration;
    }

    public void Stop()
    {
        Duration = 0;
    }
}