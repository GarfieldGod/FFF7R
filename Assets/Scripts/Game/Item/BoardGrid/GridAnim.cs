
using System;
using FFF7R;
using FFF7R.VFX;
using UnityEngine;


public enum GridAnimType
{
    None,
    Upgrade
}
[Serializable]
public class GridAnim
{
    public float Duration = 0;
    [SerializeField] private BoardCellUpgradeGroup _cellUpgrade;

    public void Play(GridAnimType type, CellDelta delta)
    {
        if (type == GridAnimType.Upgrade)
        {
            //TODO: Play upgrade animation
            _cellUpgrade.colorTint = delta.Owner == PlayerSeat.Player ? Color.white : Color.red;
            _cellUpgrade.Play(delta.After);
            Duration = _cellUpgrade.TotalDuration;
        }
    }

    public void Stop()
    {
        Duration = 0;
    }
}