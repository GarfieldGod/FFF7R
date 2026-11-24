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
    [SerializeField] float zoomSpeed = 10f;   // 缩放速度，越大越快，8~14
    [Range(1f, 3f)] [SerializeField] float miniZoomMul = 1f; // 最小缩放尺寸，1时代表Cover模式，地图宽高其中的最小值刚好撑满屏幕
    [SerializeField] float focusZoomMul    = 2f; // 聚焦时，相对于最小缩放尺寸的倍数
    [SerializeField] float maxZoomMul     = 3f;    // 最大缩放, 相对于最小缩放尺寸的倍数
    [SerializeField] float wheelZoomStep  = 1.12f; // 每格滚轮的缩放倍率
    [SerializeField] float dragRangeRatio = 0.9f; // 拖动时边界留白百分比

    [Header("移动")]
    [SerializeField] float dragSpeedRatio = 0.8f;
    [SerializeField] float moveSpeed = 6f; // 仅影响聚焦时的速度，越大越快
    [SerializeField] float slideTime = 0.2f; // 拖动结束后的速度，越大越快

    [Header("点击保护")]
    [SerializeField] float clickBlockDistance = 8f; // 拖动距离超过此值时，本帧点击无效

    // ---------------- 内部 ----------------
    Canvas _canvas;
    RectTransform _viewport; // 裁剪窗口，坐标系根节点
    Vector2 _mapSize; // 底图原始像素尺寸

    // ---------------- 缩放 ----------------
    float _scale, _targetScale;
    float _minScale = 1f;
    float _maxScale = 1f;

    // ---------------- 移动 ----------------
    bool _dragging;
    Vector2 _pos, _targetPos;

    Vector2 _dragStartPointer;
    Vector2 _dragStartPos;
    float   _dragDistance;

    Vector2 _lastDelta;
    Vector2 _velocity;

    // ---------------- 其他 ----------------
    int _clickBlockedFrame = -1;
    float _appliedScale;
    Vector2 _appliedPos;

    // ---------------- 属性 ----------------
    public bool  IsReady => _mapSize.x > 0f && _mapSize.y > 0f;
    public float Scale   => _scale;
    /// <summary>本帧是否为"刚拖完"，节点点击应据此忽略。</summary>
    public bool ClickBlocked => Time.frameCount == _clickBlockedFrame;

    // ---------------- 生命周期 ----------------

    void Awake()
    {
        _viewport = viewport != null ? viewport : (RectTransform)transform;
        _canvas = _viewport.GetComponentInParent<Canvas>();
    }

    void Start()
    {
        if (mapRoot == null) { Debug.LogError("[MapView] mapRoot 未赋值"); return; }
        if (!IsReady && mapRoot.rect.width > 0f)
            SetMapSize(mapRoot.rect.size);   // 没有外部调用时的兜底
    }

    void OnDisable()
    {
        _dragging  = false;
        _lastDelta = Vector2.zero;
        _velocity  = Vector2.zero;
    }

    void Update()
    {
        if (!IsReady) return;

        float dt = Time.unscaledDeltaTime;

        // 缩放逼近
        if (!Mathf.Approximately(_scale, _targetScale))
        {
            float k = 1f - Mathf.Exp(-zoomSpeed * dt);   // 与帧率无关的插值系数
            _scale = Mathf.Lerp(_scale, _targetScale, k);
            if (Mathf.Abs(_scale - _targetScale) < 0.0005f) _scale = _targetScale;
        }

        // 位移逼近（拖动时跳过，直接跟手）
        if (!_dragging)
        {
            if ((_pos - _targetPos).sqrMagnitude > 0.01f)
            {
                float k = 1f - Mathf.Exp(-moveSpeed * dt);
                _pos = Vector2.Lerp(_pos, _targetPos, k);
            }
            else
            {
                _pos = _targetPos;
            }
        }

        ApplyTransform();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!IsReady) return;
        RefreshRange();
        SnapScale(_scale, Vector2.zero);      // 窗口变了，重算比例并把位置拉回边界内
    }

    // ---------------- 对外 ----------------
    // mapLocalPos是某一点在地图上的坐标（相对于地图中心），与视窗无关
    public void SetFocus(Vector2 mapLocalPos, Vector2 screenOffset = default)
    {
        if (!IsReady) return;

        float focusScale = Mathf.Clamp(_minScale * focusZoomMul, _minScale, _maxScale);

        _targetScale = focusScale;
        _targetPos   = ClampPos(-mapLocalPos * focusScale + screenOffset, focusScale);
    }

    public void SetMapSize(Vector2 size)
    {
        if (size.x <= 0f || size.y <= 0f) { Debug.LogError("[MapView] mapSize 无效"); return; }
        _mapSize = size;
        mapRoot.sizeDelta = size;
        RefreshRange();
        SnapTo(_minScale, Vector2.zero);     // 初始状态不播动画
    }

    [ContextMenu("Reset View")] // Inspector 菜单
    public void ResetView()
    {
        if (!IsReady) return;
        RefreshRange();
        SnapTo(_scale, Vector2.zero);
    }

    // ---------------- 缩放与边界 ----------------

    /// <summary>最小缩放：保证底图刚好铺满视口（cover）。</summary>
    float GetCoverScale()
    {
        Vector2 v = _viewport.rect.size;
        if (v.x <= 0f || v.y <= 0f || _mapSize.x <= 0f || _mapSize.y <= 0f) return 1f;
        return Mathf.Max(v.x / _mapSize.x, v.y / _mapSize.y);
    }

    void RefreshRange()
    {
        _minScale = GetCoverScale() * miniZoomMul;
        _maxScale = _minScale * Mathf.Max(1f, maxZoomMul);
    }

    /// <summary>以 pivot（视口局部坐标）为不动点缩放到 target。</summary>
    void SnapScale(float targetScale, Vector2 pivot)
    {
        if (!IsReady) return;

        float newScale = Mathf.Clamp(targetScale, _minScale, _maxScale);
        float ratio = newScale / _scale;
        Vector2 pos = mapRoot.anchoredPosition;

        if (!Mathf.Approximately(ratio, 1f)) // Mathf.Approximately 比较浮点数是否相等
            pos = pivot - (pivot - pos) * ratio; // 缩放时以 pivot 为不动点

        _scale = newScale;
        SnapTo(_scale, ClampPos(pos, _scale));
    }

    // 限制范围，使地图始终在视口内
    Vector2 ClampPos(Vector2 pos, float s)
    {
        Vector2 v = _viewport.rect.size;
        // 地图中心的可移动范围
        Vector2 lim = new Vector2(
            Mathf.Max(0f, (_mapSize.x * s - v.x) * 0.5f * dragRangeRatio),
            Mathf.Max(0f, (_mapSize.y * s - v.y) * 0.5f * dragRangeRatio));
        return new Vector2(
            Mathf.Clamp(pos.x, -lim.x, lim.x),
            Mathf.Clamp(pos.y, -lim.y, lim.y));
    }

    /// <summary>底图加载完成后由 MapPage 调用。</summary>
    void SnapTo(float scale, Vector2 pos)
    {
        _scale = _targetScale = Mathf.Clamp(scale, _minScale, _maxScale);
        _pos   = _targetPos   = ClampPos(pos, _scale);
        ApplyTransform();
    }

    void ApplyTransform()
    {
        _pos = ClampPos(_pos, _scale); // 每帧再夹一次，防止过渡到一半露白

        if (_scale == _appliedScale && _pos == _appliedPos) return;

        _appliedScale = _scale;
        _appliedPos   = _pos;

        mapRoot.localScale       = new Vector3(_scale, _scale, 1f);
        mapRoot.anchoredPosition = _pos;
    }

    // ---------------- 拖动事件 ----------------

    public void OnBeginDrag(PointerEventData e)
    {
        if (!IsReady || !ToLocal(e, out var p)) return;
        _dragging = true;
        _dragStartPointer = p;
        _dragStartPos     = mapRoot.anchoredPosition;
        _dragDistance     = 0f;

        _velocity = Vector2.zero;
        _lastDelta = Vector2.zero;
        _targetScale = _scale; // 拖动时取消缩放过渡
    }

    public void OnDrag(PointerEventData e)
    {
        if (!IsReady || !ToLocal(e, out var p)) return;

        Vector2 delta = (p - _dragStartPointer) * dragSpeedRatio;
        if (delta.magnitude > _dragDistance) _dragDistance = delta.magnitude;

        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        _velocity = Vector2.Lerp(_velocity, (delta - _lastDelta) / dt, 0.5f);
        _lastDelta = delta;

        _targetPos = ClampPos(_dragStartPos + delta, _scale);
        _pos = _targetPos;      // 跟手：不平滑
        ApplyTransform();
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!IsReady || !ToLocal(e, out var p)) return;

        var totalDelta = _dragStartPos - p;
        _dragging = false;
        _targetPos = ClampPos(_pos + _velocity * slideTime, _scale);
        if (_dragDistance > clickBlockDistance)
            _clickBlockedFrame = Time.frameCount;   // 只挡本帧，自动过期
    }

    // ---------------- 缩放事件 ----------------

    public void OnScroll(PointerEventData e)
    {
        if (!IsReady || !ToLocal(e, out var pivot)) return;

        float scale = _targetScale * Mathf.Pow(wheelZoomStep, e.scrollDelta.y);
        float newTarget = Mathf.Clamp(scale, _minScale, _maxScale);

        if (Mathf.Approximately(newTarget, _targetScale)) return;

        // 关键：从"目标状态"推导，不是从当前状态
        float ratio = newTarget / _targetScale;

        _targetPos = pivot - (pivot - _targetPos) * ratio;
        _targetScale = newTarget;
    }

    bool ToLocal(PointerEventData e, out Vector2 local)
    {
        Camera cam = _canvas != null ? _canvas.worldCamera : null; // Overlay 时为 null
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, e.position, cam, out local);
    }
}
