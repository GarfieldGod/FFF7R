using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FFF7/Region", fileName = "Region_")]
public class RegionDef : ScriptableObject
{
    [Header("显示")]
    public string displayName = "无名地区";

    [Header("底图")]
    [Tooltip("Addressables 里的 Sprite key")]
    public string bgKey;

    [Header("关卡节点")]
    public List<LevelNodeDef> nodes = new List<LevelNodeDef>();
}