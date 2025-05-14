public enum TextColor {
    NONE,
    RED,
    YELLOW,
    BLUE,
    GREEN,
    PURPLE,
    BLACK,
    WHITE
}
public class Log {
    public static void TestLine (string info, TextColor color = TextColor.NONE, bool ifHightLight = false) {
        string textColor = GetColorText(color, ifHightLight);
    #if UNITY_ENGINE
        Debug.Log(textColor + info);
    #else
        Console.WriteLine(textColor + info);
    #endif
    }
    public static void Test (string info, TextColor color = TextColor.NONE, bool ifHightLight = false) {
        string textColor = GetColorText(color, ifHightLight);
    #if UNITY_ENGINE
        Debug.Log(textColor + info);
    #else
        Console.Write(textColor + info);
    #endif
    }
    public static void Clear() {
    #if UNITY_ENGINE
    #else
        Console.Clear();
    #endif
    }

    static string GetColorText(TextColor color, bool ifHightLight = false) {
        string result = color switch
        {
            TextColor.NONE => "\x1b[0",
            TextColor.RED => "\x1b[31",
            TextColor.YELLOW => "\x1b[33",
            TextColor.BLUE => "\x1b[34",
            TextColor.GREEN => "\x1b[32",
            TextColor.PURPLE => "\x1b[35",
            TextColor.BLACK => "\x1b[30",
            TextColor.WHITE => "\x1b[37",
            _ => "",
        };

        string highLight = "";
        if(result != "") {
            if(ifHightLight)
                highLight = ";1m";
            else
                highLight = "m";
        }
    
        result += highLight;
        return result;
    }
}

public static class Utils {
    public static readonly List<List<int>> EmptyStandard2DList = new List<List<int>> {
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
    };
    public static List<List<List<int>>> DeepCopy3DList(List<List<List<int>>> original) {
        List<List<List<int>>> result = original
        .Select(outerList => outerList
            .Select(innerList => new List<int>(innerList))
            .ToList())
        .ToList();
        return result;
    }
    public static List<List<int>> DeepCopy2DList(List<List<int>> original) {
        List<List<int>> result = original.Select(innerList => new List<int>(innerList)).ToList();
        return result;
    }
    public static List<List<int>> Compose2DList(List<List<int>> ListA, List<List<int>> ListB) {
        if(ListA.Count == 0 || ListB.Count == 0 || ListA.Count != ListB.Count) {
            return null;
        }
        if (ListA[0].Count == 0 || ListB[0].Count == 0 || ListA[0].Count != ListB[0].Count) {
            return null;
        }
        List<List<int>> result = DeepCopy2DList(ListA);
        for(int i = 0; i < result.Count; i++) {
            for(int j = 0; j < result[0].Count; j++) {
                result[i][j] += ListB[i][j];
            }
        }
        return result;
    }
}

public struct Int2D
{
    public int x;
    public int y;

    public Int2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Int2D a, Int2D b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Int2D a, Int2D b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is Int2D other)
        {
            return this == other;
        }
        return false;
    }

    public override readonly int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
}

public struct Int3D
{
    public int x;
    public int y;
    public int z;

    public Int3D(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static bool operator ==(Int3D a, Int3D b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Int3D a, Int3D b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is Int3D other)
        {
            return this == other;
        }
        return false;
    }

    public override readonly int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }
}

public struct Float3D
{
    public float x;
    public float y;
    public float z;

    public Float3D(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static bool operator ==(Float3D a, Float3D b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Float3D a, Float3D b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is Float3D other)
        {
            return this == other;
        }
        return false;
    }

    public override readonly int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }
}