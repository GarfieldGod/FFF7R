using UnityEngine;

public static class GameAssets
{
    public static readonly SpriteCatalog Sprites = new SpriteCatalog();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() {
        Sprites.Reset();
    }
}