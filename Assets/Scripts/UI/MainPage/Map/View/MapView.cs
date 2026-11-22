using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MapView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("引用")]
    [SerializeField] RectTransform mapRoot;      // 被拖动/缩放的地图层
    [SerializeField] RectTransform viewport;     // 裁剪窗口，留空=自己

    [Header("缩放")]
    [SerializeField] float miniZoomMul = 1f;
    [SerializeField] float maxZoomMul     = 3f;    // 最大缩放 = 最小缩放 × 该倍数
    [SerializeField] float wheelZoomStep  = 1.12f; // 每格滚轮的缩放倍率
    [SerializeField] float dragRangeRatio = 0.9f; // 拖动时边界留白百分比
    [SerializeField] float focusZoomMul    = 2f; // 聚焦时，底图缩放倍率

    [Header("点击保护")]
    [SerializeField] float clickBlockDistance = 8f;

    RectTransform _vp;
    Canvas _canvas;

    Vector2 _mapSize;              // 底图原始像素尺寸
    float   _scale = 1f;
    float   _minScale = 1f;
    float   _maxScale = 1f;

    Vector2 _dragStartPointer;
    Vector2 _dragStartPos;
    float   _dragDistance;

    int _clickBlockedFrame = -1;

    public bool  IsReady => _mapSize.x > 0f && _mapSize.y > 0f;
    public float Scale   => _scale;

    /// <summary>本帧是否为"刚拖完"，节点点击应据此忽略。</summary>
    public bool ClickBlocked => Time.frameCount == _clickBlockedFrame;

    // ---------------- 生命周期 ----------------

    void Awake()
    {
        _vp     = viewport != null ? viewport : (RectTransform)transform;
        _canvas = _vp.GetComponentInParent<Canvas>();
    }

    void Start()
    {
        if (mapRoot == null) { Debug.LogError("[MapView] mapRoot 未赋值"); return; }
        if (!IsReady && mapRoot.rect.width > 0f)
            SetMapSize(mapRoot.rect.size);   // 没有外部调用时的兜底
    }

    void OnRectTransformDimensionsChange()
    {
        if (!IsReady) return;
        RefreshRange();
        SetScale(_scale, Vector2.zero);      // 窗口变了，重算比例并把位置拉回边界内
    }

    // ---------------- 对外 ----------------

    /// <summary>底图加载完成后由 MapPage 调用。</summary>
    public void SetMapSize(Vector2 size)
    {
        if (size.x <= 0f || size.y <= 0f) { Debug.LogError("[MapView] mapSize 无效"); return; }
        _mapSize = size;
        mapRoot.sizeDelta = size;
        ResetView();
    }

    [ContextMenu("Reset View")] // Inspector 菜单
    public void ResetView()
    {
        if (!IsReady) return;
        RefreshRange();
        _scale = _minScale;
        SetMap(_scale, Vector2.zero);
    }

    // ---------------- 缩放与边界 ----------------

    /// <summary>最小缩放：保证底图刚好铺满视口（cover）。</summary>
    float GetCoverScale()
    {
        Vector2 v = _vp.rect.size;
        if (v.x <= 0f || v.y <= 0f || _mapSize.x <= 0f || _mapSize.y <= 0f) return 1f;
        return Mathf.Max(v.x / _mapSize.x, v.y / _mapSize.y);
    }

    void RefreshRange()
    {
        _minScale = GetCoverScale() * miniZoomMul;
        _maxScale = _minScale * Mathf.Max(1f, maxZoomMul);
    }

    /// <summary>以 pivot（视口局部坐标）为不动点缩放到 target。</summary>
    public void SetScale(float targetScale, Vector2 pivot)
    {
        if (!IsReady) return;

        float newScale = Mathf.Clamp(targetScale, _minScale, _maxScale);
        float ratio = newScale / _scale;
        Vector2 pos = mapRoot.anchoredPosition;

        if (!Mathf.Approximately(ratio, 1f)) // Mathf.Approximately 比较浮点数是否相等
            pos = pivot - (pivot - pos) * ratio; // 缩放时以 pivot 为不动点

        _scale = newScale;
        SetMap(_scale, ClampPos(pos, _scale));
    }

    // mapLocalPos是某一点在地图上的坐标（相对于地图中心），与视窗无关
    public void SetFocus(Vector2 mapLocalPos)
    {
        if (!IsReady) return;

        // var posToCenter = mapLocalPos - mapRoot.anchoredPosition; // 目标点相对于视窗中心的坐标
        // var mapCenter = mapRoot.anchoredPosition - posToCenter;
        // SetMap(_scale, mapCenter);
        // SetScale(_focusScale, Vector2.zero);

        float focusScale = Mathf.Clamp(_minScale * focusZoomMul, _minScale, _maxScale);

        // 目标点居中 → 地图偏移取反并乘以缩放
        Vector2 pos = ClampPos(-mapLocalPos * focusScale, focusScale);

        _scale = focusScale;
        // SetMap(focusScale, pos);

        mapRoot.DOKill();
        mapRoot.DOScale(new Vector3(focusScale, focusScale, 1f), 1f).SetEase(Ease.OutCubic);
        mapRoot.DOAnchorPos(pos, 1).SetEase(Ease.OutCubic);
    }

    void SetMap(float scale, Vector2 pos)
    {
        mapRoot.localScale = new Vector3(scale, scale, 1f);
        mapRoot.anchoredPosition = pos;
    }

    // 限制范围，使地图始终在视口内
    Vector2 ClampPos(Vector2 pos, float s)
    {
        Vector2 v = _vp.rect.size;
        // 地图中心的可移动范围
        Vector2 lim = new Vector2(
            Mathf.Max(0f, (_mapSize.x * s - v.x) * 0.5f * dragRangeRatio),
            Mathf.Max(0f, (_mapSize.y * s - v.y) * 0.5f * dragRangeRatio));
        return new Vector2(
            Mathf.Clamp(pos.x, -lim.x, lim.x),
            Mathf.Clamp(pos.y, -lim.y, lim.y));
    }

    // ---------------- 拖动事件 ----------------

    public void OnBeginDrag(PointerEventData e)
    {
        mapRoot.DOKill();
        if (!IsReady || !ToLocal(e, out var p)) return;
        _dragStartPointer = p;
        _dragStartPos     = mapRoot.anchoredPosition;
        _dragDistance     = 0f;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!IsReady || !ToLocal(e, out var pos)) return;

        Vector2 delta = pos - _dragStartPointer;
        if (delta.magnitude > _dragDistance) _dragDistance = delta.magnitude;

        mapRoot.anchoredPosition = ClampPos(_dragStartPos + delta, _scale);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_dragDistance > clickBlockDistance)
            _clickBlockedFrame = Time.frameCount;   // 只挡本帧，自动过期
    }

    // ---------------- 缩放事件 ----------------

    public void OnScroll(PointerEventData e)
    {
        mapRoot.DOKill();
        if (!IsReady || !ToLocal(e, out var pos)) return;
        SetScale(_scale * Mathf.Pow(wheelZoomStep, e.scrollDelta.y), pos);
    }

    bool ToLocal(PointerEventData e, out Vector2 local)
    {
        Camera cam = _canvas != null ? _canvas.worldCamera : null; // Overlay 时为 null
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(_vp, e.position, cam, out local);
    }
}
