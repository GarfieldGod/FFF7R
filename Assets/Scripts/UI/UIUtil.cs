using UnityEngine.UI;

public static class UIUtils
{
    public static void SetAlpha(Image image, float a)
    {
        var c = image.color;
        c.a = a;
        image.color = c;
    }
}
