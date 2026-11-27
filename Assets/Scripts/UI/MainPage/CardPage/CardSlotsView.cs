using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlotsView : MonoBehaviour
{
    [SerializeField] Transform root;
    [SerializeField] CardUISlot cardSlotPrefab;
    [SerializeField] float slotScale;
    // Start is called before the first frame update

    List<CardUISlot> slots = new List<CardUISlot>();

    public event Action<string> OnCardClick;

    void OnDestroy()
    {
        foreach (var slot in slots)
        {
            slot.Clicked -= OnClick;
            Destroy(slot.gameObject);
        }
    }

    void OnClick(string cardCode) => OnCardClick?.Invoke(cardCode);

    public void ShowCards(IReadOnlyList<string> slotSource, IReadOnlyList<string> countSource)
    {
        if (slotSource == null) { HideFrom(0); return; }

        var owned = Tally(slotSource);    // code → 拥有数（同时就是槽位全集 + max）
        var cur   = Tally(countSource);   // code → 可用数

        var items = new List<(string Code, int Cur, int Max)>(owned.Count);

        // 按全局稳定顺序输出
        foreach (var code in App.Cards.AllCodes)
        {
            if (!owned.TryGetValue(code, out int max)) continue;
            owned.Remove(code);
            items.Add((code, cur.TryGetValue(code, out int n) ? n : 0, max));
        }

        Render(items);
    }

    static Dictionary<string, int> Tally(IReadOnlyList<string> src)
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
        EnsureSlots(items.Count);
        int n = items.Count;

        for (int i = 0; i < n; i++)
        {
            var slot = slots[i];
            if (!slot.gameObject.activeSelf) slot.gameObject.SetActive(true);
            if (slot.CardCode != items[i].Code) slot.Bind(items[i].Code);
            slot.SetNum(items[i].Cur, items[i].Max);
        }

        HideFrom(n);
    }

    // public void ShowCards(IReadOnlyList<string> slotSource, IReadOnlyList<string> countSource)
    // {
    //     if (slotSource == null) { HideFrom(0); return; }

    //     var stacks = Group(slotSource);
    //     EnsureSlots(stacks.Count);
    //     int n = Mathf.Min(stacks.Count, slots.Count);

    //     if (stacks.Count > slots.Count)
    //         Debug.LogWarning($"[{name}] 需要 {stacks.Count} 槽，当前只有 {slots.Count}");

    //     for (int i = 0; i < n; i++)
    //     {
    //         var slot = slots[i];
    //         if (!slot.gameObject.activeSelf) slot.gameObject.SetActive(true);
    //         if (slot.CardCode != stacks[i].Code) slot.Bind(stacks[i].Code);
    //         slot.SetNum(stacks[i].Count);
    //     }

    //     HideFrom(n);
    // }

    public void HideFrom(int index)
    {
        for (int i = index; i < slots.Count; i++)
            if (slots[i].gameObject.activeSelf) slots[i].gameObject.SetActive(false);
    }

    List<(string Code, int Count)> Group(IReadOnlyList<string> cards)
    {
        var tmp = new Dictionary<string, int>(cards.Count);
        foreach (var c in cards)
        {
            if (c == null) continue;
            tmp.TryGetValue(c, out int n);
            tmp[c] = n + 1;
        }

        var result = new List<(string Code, int Count)>(tmp.Count);
        foreach (var code in App.Cards.AllCodes)
        {
            if (tmp.TryGetValue(code, out int n)) result.Add((code, n));
        }
        return result;
    }

    void EnsureSlots(int need)
    {
        while (slots.Count < need)
        {
            var slot = Instantiate(cardSlotPrefab, root, false);
            slot.transform.localScale = Vector3.one * slotScale;
            slot.gameObject.SetActive(false);
            slots.Add(slot);
            slot.Clicked += OnClick;
        }
    }
}
