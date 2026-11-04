using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public class ScoreGrid : MonoBehaviour
{
    [SerializeField] private PowerNumDisplay num;

    public void SetScore(int value) => num.SetNumber(value);
}
