
using System;
using FFF7R.VFX;
using UnityEngine;

[Serializable]
public class CardFx
{
    [SerializeField] private CardAuraFX AuraFX;
    int _active = -1;
    static readonly Color Negative = new Color(1, 0, 1, 1);
    static readonly Color Positive = new Color(0, 1, 84/255f, 1);

    public void Hide(bool immediately = false)
    {
        if (_active == 0) return;
        _active = 0;

        if (immediately) AuraFX.SnapOn(false);
        else AuraFX.SetOn(false);
    }

    public void Show(bool immediately = false)
    {
        if (_active == 1) return;
        _active = 1;

        if (immediately) AuraFX.SnapOn(true);
        else AuraFX.SetOn(true);
    }

    public void SetActivePositive(bool active, bool positive, bool immediately = false)
    {
        AuraFX.tint = positive ? Positive : Negative;
        SetActive(active, immediately);
    }

    public void SetActive(bool active, bool immediately = false)
    {
        if (active) Show(immediately);
        else Hide(immediately);
    }
}