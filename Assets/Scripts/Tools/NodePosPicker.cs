using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class NodePosPicker : MonoBehaviour
{
    [SerializeField] RectTransform mapPic;
    [SerializeField] Vector2 normalizedPos;

    void FlushNormalized()
    {
        if (mapPic == null) return;

        Vector2 size = mapPic.rect.size;
        Vector2 lp   = transform.localPosition;   // 相对 MapPic 的 pivot（在中心）

        normalizedPos = new Vector2(
            0.5f + lp.x / size.x,
            0.5f + lp.y / size.y);
    }

    [ContextMenu("打印归一化坐标")]
    void PrintNormalized()
    {
        Debug.Log($"[{name}] ({normalizedPos.x:F4}f, {normalizedPos.y:F4}f) size={mapPic.rect.size}", this);
    }

    void Update()
    {
        FlushNormalized();
    }
}
#endif