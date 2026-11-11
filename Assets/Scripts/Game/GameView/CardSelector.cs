using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFF7R;
using UnityEngine;

public class CardSelector : MonoBehaviour, IGameView
{
    public List<Card> cards = new List<Card>();
    public SeatAction Action => select.Action;
    public Action<Card> FocusChanged;

    [Header("设置")]
    [SerializeField] private CardBoard board;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform DispenseCardSrc;
    [SerializeField] private Camera cam;
    [SerializeField] private CardReDispense reDispense = new CardReDispense();
    [SerializeField] private CardLayout layout = new CardLayout();
    [SerializeField] private CardSelect select = new CardSelect();
    private CardHover hover = new CardHover();

    public PlayerSeat seat = PlayerSeat.Player;
    public bool isLocal = true;
    public bool faceUp = true;

    private IGameQuery _gameQuery;
    private Quaternion ForwardToCam;

    private List<Card> _unselected = new List<Card>();
    private int  _lastHand = -1;
    private Int2 _lastPos  = Int2.Unknown;
    private bool _hadPreview;
    private Card _lastFocus;
    private List<Card> _canInput = new List<Card>();

    public void Start()
    {
        ForwardToCam = isLocal ? 
            cam.transform.rotation * Quaternion.Euler(0, 180f, 0f) :
            cam.transform.rotation * Quaternion.Euler(0, 180f, 180f);

        transform.rotation = ForwardToCam;
    }

    public void Bind(IGameQuery gameQuery)
    {
        _gameQuery = gameQuery;
        Resync();
    }

    public void Unbind()
    {
        _gameQuery = null;
    }

    public void Resync()
    {
        if (_gameQuery == null) return;
        UpdateHand(_gameQuery.Hand(seat));
    }

    public void OnEvents(GameEventBatch batch)
    {
        if (_gameQuery == null) return;
        UpdateHand(_gameQuery.Hand(seat));
    }

    public void UpdateHand(HandSnapshot hand)
    {
        var pool = new List<CardView>(hand.Cards);
        var keep = new List<Card>(cards.Count);

        foreach (Card c in cards)
        {
            int idx = pool.FindIndex(v => v.CardCode == c.CardCode);
            if (idx < 0)
            {
                // 快照里没有 → 它已经不在这手牌里（打出了 / 被效果去掉）
                if (c == select.Current) select.Current = null;   // 别走 UnSelect，它会碰 Collider
                if (c == _lastFocus) { _lastFocus = null; FocusChanged?.Invoke(null); }
                reDispense.Selected.Remove(c);
                Destroy(c.gameObject);
                continue;
            }

            c.HandIndex = pool[idx].HandIndex;
            pool.RemoveAt(idx);
            keep.Add(c);
        }

        cards.Clear();
        cards.AddRange(keep);

        foreach (CardView v in pool) AddCard(v);
    }

    public void AddCard(CardView cardView)
    {
        Card card = Instantiate(cardPrefab, transform);
        card.SetCardInHand(cardView, seat, faceUp);
        card.transform.position = DispenseCardSrc.position;
        cards.Add(card);
    }

    // --------------------------------------------------------------------------------------------------------TickSelect
    public void TickSelect()
    {
        if (isLocal && _gameQuery.State.IsGaming) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject hitObj = null;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                hitObj = hit.collider.gameObject;
            }

            CardSelect.TickContext ctx = new CardSelect.TickContext {
                gameQuery = _gameQuery,
                cards = cards,
                canInput = _canInput,
                hit = hitObj,
                cam = cam,
                facing = ForwardToCam,
                baseScale = layout.baseScale,
                RefreshPlayableFx = RefreshPlayableFx,
                seat = seat,
                board = board
            };

            select.Tick(ctx, out var pos);

            bool want = select.Current != null && pos != Int2.Unknown;
            if (!want)
            {
                if (_hadPreview) { board.ClearPreview(); _hadPreview = false; }
                _lastHand = -1; _lastPos = Int2.Unknown;
                return;
            }

            bool changed = !_hadPreview || pos != _lastPos || select.Current.HandIndex != _lastHand;
            if (!changed) return;                       // 目标没变，什么都不做

            var snapshot = _gameQuery.Preview(seat, select.Current.HandIndex, pos);
            _lastHand = select.Current.HandIndex;
            _lastPos  = pos;

            if (snapshot == null)
            {
                _hadPreview = false;
                board.ClearPreview();
                return;
            }
            _hadPreview = true;
            board.ShowPreview(_gameQuery.Preview(seat, _lastHand, pos));
        }
    }

    public SeatAction TakeAction()
    {
        var a = select.Action;
        select.Action = SeatAction.None;
        return a;
    }

    public void OnInputSuccess() => select.UnSelect();

    // --------------------------------------------------------------------------------------------------------TickReDispense
    public void TickReDispense()
    {
        if (!isLocal)
        {
            layout.Refresh(cards, null, null, ForwardToCam);
            return;
        }
        if (_gameQuery.State.Status != MatchStatus.ReDispense) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        GameObject hitObj = null;
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
            hitObj = hit.collider.gameObject;
        }
        hover.Tick(cards, hitObj, cam, false);                  // ← 新增。换牌没有拖拽，selecting 恒 false
        reDispense.Tick(cards, hitObj, ForwardToCam, 
            hover.Current, layout.baseScale, layout.hoverScale);          // 点击切换选中

        _unselected.Clear();                                     // ← 复用成员 List，别每帧 Where().ToList()
        foreach (var c in cards) {
            bool selected = reDispense.Selected.Contains(c);
            if (!selected) _unselected.Add(c);
            c.Fx.SetBanFx(selected);
        }

        layout.Refresh(_unselected, null, hover.Current, ForwardToCam);   // ← 第三个参数不再传 null
        UpdateFocus();
    }

    public List<int> GetReDispense() => reDispense.Selected.Select(c => c.HandIndex).ToList();
    public void ClearReDispense() => reDispense.Selected.Clear();

    // --------------------------------------------------------------------------------------------------------TickHover
    public void TickHover()
    {
        if (isLocal) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject hitObj = null;
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Collide)) {
                hitObj = hit.collider.gameObject;
            }

            hover.Tick(cards, hitObj, cam, select.Current != null);
        }
    }

    //--------------------------------------------------------------------------------------------------------Tick
    public void TurnStart()
    {
        if (_gameQuery == null) return;
        _canInput.Clear();

        var st = _gameQuery.State;
        bool active = isLocal && faceUp && st.IsGaming && st.Current == seat;

        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            bool ok = active
                && c.CardCode != "__back"
                && HasAnyLegalPos(c.HandIndex);

            if (ok) _canInput.Add(c);
        }

        RefreshPlayableFx();
    }

    public void TurnTick()
    {
        TickSelect();
    }
    public void TurnEnd()
    {
        ClearPlayableFx();
    }

    // 非己方回合也Tick的事件
    void LateUpdate()
    {
        if (_gameQuery == null) return;
        var st = _gameQuery.State;

        if (st.Status == MatchStatus.ReDispense)
        {
            TickReDispense();
            return;
        }

        if (!st.IsGaming) return;
        TickHover();

        layout.Refresh(cards, select.Current, hover.Current, ForwardToCam);
        UpdateFocus();
    }

    void UpdateFocus()
    {
        Card focus = select.Current != null ? select.Current : hover.Current;
        if (focus == _lastFocus) return;
        _lastFocus = focus;
        FocusChanged?.Invoke(focus);
    }

    void RefreshPlayableFx()
    {
        foreach(var c in cards) {
            if (c != select.Current && _canInput.Contains(c)) c.Fx.SetAuraFx(true);
            else c.Fx.SetAuraFx(false);
        }
    }

    void ClearPlayableFx()
    {
        for (int i = 0; i < cards.Count; i++) cards[i].Fx.SetAuraFx(false);
    }

    bool HasAnyLegalPos(int handIndex)
    {
        var b = _gameQuery.Board;
        for (int x = 0; x < b.Width; x++)
            for (int y = 0; y < b.Height; y++)
                if (_gameQuery.CanPlace(seat, handIndex, new Int2(x, y), out _))
                    return true;
        return false;
    }
}