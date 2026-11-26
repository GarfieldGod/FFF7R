using UnityEngine;
using UnityEngine.UI;

public class MapPage : MonoBehaviour
{
    [SerializeField] WorldDef world;

    [SerializeField] Image mapImage;

    [SerializeField] MapView mapView;
    [SerializeField] MapNodeLayer mapNodeLayer;
    [SerializeField] MapCursor mapCursor;

    int _token;

    void Start()
    {
        LoadMap();

        mapNodeLayer.NodeClicked += OnNodeClick;
    }

    void OnDestroy()
    {
        mapNodeLayer.NodeClicked -= OnNodeClick;
    }

    void LoadMap()
    {
        if (world == null || mapView == null || mapImage == null || mapNodeLayer == null)
        {
            Debug.LogError("[MapPage] 引用未配齐");
            return;
        }

        if (world.regions == null || world.regions.Count == 0) {
            Debug.LogError("[MapPage] 区域未配置");
            return;
        }

        RegionDef region = world.regions[0];
        string mapPic = region.bgKey;

        // 加载底图
        int token = ++_token;
        GameAssets.Sprites.Request(mapPic, sprite =>
        {
            if (token != _token) return;
            if (sprite == null)
            {
                Debug.LogError($"[MapPage] 底图加载失败: {mapPic}");
                return;
            }

            mapImage.sprite = sprite;
            mapView.SetMapSize(sprite.rect.size);

            mapNodeLayer.Build(region, sprite.rect.size);
        });
    }

    void OnNodeClick(LevelNodeDef def, Vector2 pos)
    {
        Debug.Log($"[MapPage] 点击节点: {def.levelId}");
        mapView.SetFocus(pos);
        mapCursor.EnableCursor(pos);
    }
}
