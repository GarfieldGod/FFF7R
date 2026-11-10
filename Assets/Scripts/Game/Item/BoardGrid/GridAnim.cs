
using System;
using DG.Tweening;
using FFF7R;
using FFF7R.VFX;
using UnityEngine;

[Serializable]
public class GridAnim
{
    [Header("Cell Upgrade")]
    [SerializeField] private BoardCellUpgradeGroup _cellUpgrade;
    [Header("Card Placed")]
    [SerializeField] private BoardCellUpgradeGroup _cardPlaced;
    [SerializeField] private float PlaceLiftSpeed = 0.5f;
    [SerializeField] private float PlaceDownSpeed = 0.5f;
    [SerializeField] private float PlaceStart = 2;
    [SerializeField] private float PlaceLift = 4;
    [SerializeField] private float PlaceFinal = 0.5f;
    [Header("Card Upgrade")]
    [SerializeField] private BoardCellUpgradeGroup _cardUpgrade;

    private BoardGrid _boardGrid;

    public float CellUpgradeDuration => _cellUpgrade != null ? _cellUpgrade.TotalDuration : 0;
    public float FlightDuration => PlaceLiftSpeed + PlaceDownSpeed;
    public float PlaceVfxDuration => _cardPlaced != null ? _cardPlaced.TotalDuration : 0f;
    public float CardUpgradeVfxDuration => _cardUpgrade != null ? _cardUpgrade.TotalDuration : 0f;

    public void SetBoardGrid(BoardGrid boardGrid)
    {
        _boardGrid = boardGrid;
    }

    // ----------------------------------------------------------------------------------------CellUpgrade
    public void CellUpgradeVFX(CellDelta delta)
    {
        _cellUpgrade.colorTint = delta.Owner == PlayerSeat.Player ? Color.white : Color.red;
        _cellUpgrade.Play(delta.After);
    }

    public void StopCellUpgradeVFX() {}

    // ----------------------------------------------------------------------------------------CardPlace
    public void CardPlaceVFX()
    {
        _cardPlaced.Play();
    }

    public void StopCardPlaceVFX() {}

    public void CardPlaceFlight(float elapsed)
    {
        if (_boardGrid == null || _boardGrid.currentCard == null) return;

        float up = Mathf.Max(PlaceLiftSpeed, 1e-4f);
        float down = Mathf.Max(PlaceDownSpeed, 1e-4f);
        float y;

        if (elapsed < up)
        {
            float k = Mathf.Clamp01(elapsed / up);
            k = 1f - (1f - k) * (1f - k);                 // OutQuad
            y = Mathf.Lerp(PlaceStart, PlaceLift, k);
        }
        else
        {
            float k = Mathf.Clamp01((elapsed - up) / down);
            k *= k;                                        // InQuad
            y = Mathf.Lerp(PlaceLift, PlaceFinal, k);
        }

        var t = _boardGrid.currentCard.transform;
        t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);   // 保留 x/z
    }

    public void StopCardPlaceFlight()
    {
        if (_boardGrid == null || _boardGrid.currentCard == null) return;
        _boardGrid.currentCard.transform.localPosition = new Vector3(0, PlaceFinal, 0);
    }

    // ----------------------------------------------------------------------------------------CardUpgrade

    public void CardUpgradeVFX(int value)
    {
        _cardUpgrade.colorTint = value > 0 ? new Color(0, 1, 62/255f, 1) : new Color(1, 0, 1, 1);
        _cardUpgrade.Play();
    }

    public void StopCardUpgradeVFX() {}
}