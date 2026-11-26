using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavUI : MonoBehaviour
{
    [SerializeField] private int defaultIndex = 0;
    [SerializeField] private NavButton[] navButtons;

    public Action<int> OnNavButtonSelected;

    void Awake()
    {
        foreach (var button in navButtons)
        {
            button.Deselect(false);
            button.OnSelect += SelectButton;
        }

        if (navButtons.Length > defaultIndex)
            SelectButton(navButtons[defaultIndex]);
    }

    void OnDestroy()
    {
        foreach (var button in navButtons)
            button.OnSelect -= SelectButton;
    }

    public void SelectButton(NavButton btn)
    {
        foreach (var button in navButtons) button.Deselect();
        int index = Array.IndexOf(navButtons, btn);
        if (index != -1) btn.Select();
        OnNavButtonSelected?.Invoke(index);
    }
}
