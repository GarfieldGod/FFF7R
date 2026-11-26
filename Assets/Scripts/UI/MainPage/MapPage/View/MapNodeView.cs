using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MapNodeState { Locked, Available, Finished }

[RequireComponent(typeof(Image))]
public class MapNodeView : MonoBehaviour, IPointerClickHandler
{
    static readonly string LEVEL_NODE_ICON_AVAILABLE_PATH = "_00";
    static readonly string LEVEL_NODE_ICON_FINISHED_PATH = "_01";

    [SerializeField] float _baseAlpha = 1f;
    [SerializeField] float _fadeInTime = 0.5f;

    [SerializeField] List<string> levelNodeIcons = new List<string>();
    Image _iconImage;
    int _token = 0;

    bool _ready;
    LevelNodeDef _def;
    MapNodeState _state;
    MapView _mapView;
    Action<LevelNodeDef> _onClick;

    void Awake()
    {
        _iconImage = GetComponent<Image>();

        UIUtils.SetAlpha(_iconImage, 0);
    }

    public void Bind(LevelNodeDef def, MapNodeState state, MapView mapView, Action<LevelNodeDef> onClick)
    {
        if (state == MapNodeState.Locked) return;
        _def = def;
        _state = state;
        _mapView = mapView;
        _onClick = onClick;

        if (levelNodeIcons.Count == 0)
        {
            Debug.LogError("levelNodeIcons is empty");
            return;
        }

        // 加载图标
        bool invalid = def.level < 0 || def.level >= levelNodeIcons.Count;
        if (invalid) Debug.LogError("Invalid level " + def.level);

        int level = invalid ? 0 : def.level;
        string iconName = levelNodeIcons[level] + 
            (state == MapNodeState.Available ? LEVEL_NODE_ICON_AVAILABLE_PATH : LEVEL_NODE_ICON_FINISHED_PATH);

        int token = ++_token;
        GameAssets.Sprites.Request(iconName, (s) =>
        {
            if (token != _token) return;        // 已被更新的 Bind 取代
            if (this == null || _iconImage == null) return;

            if (s == null)
            {
                Debug.LogError($"[MapNodeView] 图标加载失败: {iconName}", this);
                return;
            }

            _iconImage.sprite = s;
            _ready = true;
            FadeIn();
        });
    }

    void FadeIn()
    {
        _iconImage.DOKill();
        _iconImage.DOFade(_baseAlpha, _fadeInTime).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_ready || _def == null) return;
        if (_state == MapNodeState.Locked) return;

        _onClick?.Invoke(_def);
    }
}