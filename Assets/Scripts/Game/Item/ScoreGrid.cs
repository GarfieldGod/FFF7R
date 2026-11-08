using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public class ScoreGrid : MonoBehaviour
{
    [SerializeField] private PowerNumDisplay num;
    [SerializeField] private CanvasGroup group;

    public void SetScore(int value) => num.SetNumber(value);

    public bool Invalid
    {
        set { group.alpha = value ? 0.2f : 1f; }
    }
}