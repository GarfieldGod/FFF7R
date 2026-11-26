using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeLayer : MonoBehaviour
{
    [SerializeField] MapView mapView;
    [SerializeField] RectTransform nodeRoot;      // 上面那个 Nodes
    [SerializeField] MapNodeView nodePrefab;

    public event Action<LevelNodeDef, Vector2> NodeClicked;

    readonly List<MapNodeView> _spawned = new();

    public void Build(RegionDef region, Vector2 mapSize)
    {
        Clear();

        foreach (var def in region.nodes)
        {
            // 第三参数必须是 false
            var nodeView = Instantiate(nodePrefab, nodeRoot, false);

            var rt = (RectTransform)nodeView.transform;
            var pos = new Vector2(
                (def.normalizedPos.x - 0.5f) * mapSize.x,
                (def.normalizedPos.y - 0.5f) * mapSize.y);

            rt.anchoredPosition = pos;

            nodeView.Bind(def, MapNodeState.Available, mapView,
                (def) => NodeClicked?.Invoke(def, pos));
            _spawned.Add(nodeView);
        }
    }

    public void Clear()
    {
        foreach (var v in _spawned)
            if (v != null) Destroy(v.gameObject);
        _spawned.Clear();
    }
}
