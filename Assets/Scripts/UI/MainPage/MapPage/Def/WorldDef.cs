using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FFF7/World")]
public class WorldDef : ScriptableObject
{
    public string worldId;
    public List<RegionDef> regions;
}