
using System;
using FFF7R.VFX;
using UnityEngine;

[Serializable]
public class CardFx
{
    [SerializeField] private CardAuraFX AuraFX;
    [SerializeField] private CardBanFX BanFX;

    int _auraFxActive = -1;
    static readonly Color Negative = new Color(1, 0, 1, 1);
    static readonly Color Positive = new Color(0, 1, 84/255f, 1);

    public void SetAuraFx(bool active, bool positive = true, bool immediately = false)
    {
        if (active)
        {
            if (_auraFxActive == 1) return;
            _auraFxActive = 1;
            AuraFX.tint = positive ? Positive : Negative;

            if (immediately) AuraFX.SnapOn(true);
            else AuraFX.SetOn(true);
        }
        else
        {
            if (_auraFxActive == 0) return;
            _auraFxActive = 0;

            if (immediately) AuraFX.SnapOn(false);
            else AuraFX.SetOn(false);
        }
    }

    public void SetBanFx(bool active)
    {
        BanFX.SetOn(active);
    }

    public void PlayBanFxOnce(float seconds = 0)
    {
        BanFX.PlayFor();
    }
}