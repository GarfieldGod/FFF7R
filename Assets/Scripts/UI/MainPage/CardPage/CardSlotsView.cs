using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotsView : MonoBehaviour
{
    [SerializeField] Transform root;
    [SerializeField] CardUISlot cardSlotPrefab;
    [SerializeField] float slotScale;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Image scrollHandle;

    List<CardUISlot> slots = new List<CardUISlot>();
    Coroutine _buildCo;

    public event Action<string> OnCardClick;

    public void ShowCards(IReadOnlyList<string> slotSource, IReadOnlyList<string> countSource)
    {
        if (slotSource == null) { HideFrom(0); return; }

        var data = BuildSlotsData(slotSource, countSource);

        Render(data);
    }

    public void ScrollToTop()
    {
        if (scrollRect == null) return;
        scrollRect.StopMovement();                                   // ① 清掉滑动惯性
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            (RectTransform)scrollRect.content);                      // ② 先把布局算完
        scrollRect.verticalNormalizedPosition = 1f;                   // ③ 1 = 顶部
    }

    void OnDestroy()
    {
        foreach (var slot in slots)
        {
            slot.Clicked -= OnClick;
            Destroy(slot.gameObject);
        }
    }

    void OnClick(string cardCode) => OnCardClick?.Invoke(cardCode);

    List<(string Code, int Cur, int Max)> BuildSlotsData(IReadOnlyList<string> slotSource, IReadOnlyList<string> countSource)
    {
        var owned = CountCardNum(slotSource);    // code → 拥有数（同时就是槽位全集 + max）
        var cur   = CountCardNum(countSource);   // code → 可用数

        var items = new List<(string Code, int Cur, int Max)>(owned.Count);

        // 按全局稳定顺序输出
        foreach (var code in App.Cards.AllCodes)
        {
            if (!owned.TryGetValue(code, out int max)) continue;
            owned.Remove(code);
            items.Add((code, cur.TryGetValue(code, out int n) ? n : 0, max));
        }

        return items;
    }

    static Dictionary<string, int> CountCardNum(IReadOnlyList<string> src)
    {
        var d = new Dictionary<string, int>(src?.Count ?? 0);
        if (src == null) return d;
        foreach (var c in src)
        {
            if (string.IsNullOrEmpty(c)) continue;
            d.TryGetValue(c, out int n);
            d[c] = n + 1;
        }
        return d;
    }

    void Render(List<(string Code, int Cur, int Max)> items)
    {
        if (_buildCo != null) { StopCoroutine(_buildCo); _buildCo = null; }
        int common = Mathf.Min(items.Count, slots.Count);

        bool codeChanged = false;
        for (int i = 0; i < common; i++)
            if (slots[i].CardCode != items[i].Code) { codeChanged = true; break; }

        bool needRebuild = codeChanged || items.Count > slots.Count;
        if (needRebuild) HideHandle();
        else ShowHandle();

        if (!codeChanged)
        {
            for (int i = 0; i < common; i++)
            {
                if (!slots[i].gameObject.activeSelf) slots[i].gameObject.SetActive(true);
                slots[i].SetNum(items[i].Cur, items[i].Max);
            }
            HideFrom(common);                       // 点卡场景：common 不变，这里空转
            if (items.Count > common)               // 只有真的追加了新卡才需要建
                _buildCo = StartCoroutine(BuildRest(items, common));
            else
                ShowHandle();
            return;
        }

        if (scrollHandle != null) UIUtils.SetAlpha(scrollHandle, 0f);

        const int MinCount = 22;
        int n = Mathf.Min(items.Count, MinCount);

        // 创建起始
        EnsureSlots(n);
        for (int i = 0; i < n; i++) BindAt(i, items[i]);
        HideFrom(n);

        // 创建并绑定剩下的
        if (items.Count > n)
            _buildCo = StartCoroutine(BuildRest(items, n));
        else
            ShowHandle();
    }

    void BindAt(int i, (string Code, int Cur, int Max) item)
    {
        var slot = slots[i];
        if (!slot.gameObject.activeSelf) slot.gameObject.SetActive(true);
        if (slot.CardCode != item.Code) slot.Bind(item.Code);
        slot.SetNum(item.Cur, item.Max);
    }

    IEnumerator BuildRest(List<(string Code, int Cur, int Max)> items, int start)
    {
        const double BUDGET_MS = 8.0;      // 每帧最多花 8ms，留 8ms 给渲染
        var sw = System.Diagnostics.Stopwatch.StartNew();

        int i = start;
        while (i < items.Count)
        {
            sw.Restart();
            while (i < items.Count && sw.Elapsed.TotalMilliseconds < BUDGET_MS)
            {
                if (slots.Count <= i) EnsureSlots(i + 1);
                BindAt(i, items[i]);
                i++;
            }
            HideFrom(i);
            yield return null;
        }
        _buildCo = null;
        ShowHandle(instant: false);
    }

    public void HideFrom(int startIndex)
    {
        for (int i = startIndex; i < slots.Count; i++)
            if (slots[i].gameObject.activeSelf) slots[i].gameObject.SetActive(false);
    }

    void EnsureSlots(int count)
    {
        while (slots.Count < count)
        {
            var slot = Instantiate(cardSlotPrefab, root, false);
            slot.transform.localScale = Vector3.one * slotScale;
            slot.Clicked += OnClick;
            slots.Add(slot);
        }
    }

    void HideHandle()
    {
        if (scrollHandle == null) return;
        scrollHandle.DOKill();
        UIUtils.SetAlpha(scrollHandle, 0f);
    }

    void ShowHandle(bool instant = true)
    {
        if (scrollHandle == null) return;
        scrollHandle.DOKill();
        if (instant) UIUtils.SetAlpha(scrollHandle, 1f);
        else scrollHandle.DOFade(1f, 0.5f).SetLink(gameObject);
    }
}
