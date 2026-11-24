using FFF7R;
using FFF7R.Adapter;
using UnityEngine;

public static class App
{
    static CardCatalog _cards;

    public static ICardCatalog Cards => _cards ??= CreateCardCatalog();

    static CardCatalog CreateCardCatalog()
    {
        var c = new CardCatalog();
        c.Load();          // 也可以不调，首次访问时 EnsureLoaded 会自动加载
        return c;
    }

    // 关掉 Domain Reload 时静态字段不会重置
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() => _cards = null;
}
