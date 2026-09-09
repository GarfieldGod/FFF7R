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
    public static void TestLine(string info, TextColor color = TextColor.NONE, bool ifHightLight = false)
    {
        string textColor = GetColorText(color, ifHightLight);
#if UNITY_ENGINE
        Debug.Log(textColor + info);
#else
        Test(info, color, ifHightLight);
        Test("\n");
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
        if (Environment.UserInteractive && Console.In != StreamReader.Null)
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {

            }
        }
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
        if (result != "") {
            if (ifHightLight)
                highLight = ";1m";
            else
                highLight = "m";
        }
    
        result += highLight;
        return result;
    }
}

public static class Utils
{
    public static List<List<int>> NewEmpty2DList(int lengh, int height)
    {
        List<List<int>> result = new List<List<int>> { };
        for (int x = 0; x < height; x++)
        {
            List<int> line = new List<int>();
            for (int y = 0; y < lengh; y++)
            {
                line.Add(0);
            }
            result.Add(line);
        }
        return result;
    }
    public static readonly List<List<int>> EmptyStandard2DList = new List<List<int>> {
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
        new List<int> { 0, 0, 0, 0, 0 },
    };
    public static List<List<List<int>>> DeepCopy3DList(List<List<List<int>>> original)
    {
        List<List<List<int>>> result = original
        .Select(outerList => outerList
            .Select(innerList => new List<int>(innerList))
            .ToList())
        .ToList();
        return result;
    }
    public static List<List<int>> DeepCopy2DList(List<List<int>> original)
    {
        List<List<int>> result = original.Select(innerList => new List<int>(innerList)).ToList();
        return result;
    }
    public static List<List<int>> Compose2DList(List<List<int>> ListA, List<List<int>> ListB)
    {
        if (ListA.Count == 0 || ListB.Count == 0 || ListA.Count != ListB.Count)
        {
            return null;
        }
        if (ListA[0].Count == 0 || ListB[0].Count == 0 || ListA[0].Count != ListB[0].Count)
        {
            return null;
        }
        List<List<int>> result = DeepCopy2DList(ListA);
        for (int i = 0; i < result.Count; i++)
        {
            for (int j = 0; j < result[0].Count; j++)
            {
                result[i][j] += ListB[i][j];
            }
        }
        return result;
    }
    public static List<List<List<Buff>>> DeepCopy(this List<List<List<Buff>>> original)
    {
        if (original == null)
        {
            return null;
        }
        var deepCopiedList = new List<List<List<Buff>>>();
        foreach (var secondLevelList in original)
        {
            var secondLevelCopiedList = new List<List<Buff>>();
            foreach (var thirdLevelList in secondLevelList)
            {
                var thirdLevelCopiedList = new List<Buff>();
                foreach (var buff in thirdLevelList)
                {
                    var copiedBuff = new Buff(buff.source, buff.value, buff.scope, buff.type);
                    thirdLevelCopiedList.Add(copiedBuff);
                }
                secondLevelCopiedList.Add(thirdLevelCopiedList);
            }
            deepCopiedList.Add(secondLevelCopiedList);
        }
        return deepCopiedList;
    }
    public static List<List<PadGrid>> DeepCopy(this List<List<PadGrid>> original)
    {
        if (original == null)
        {
            return null;
        }
        var deepCopiedList = new List<List<PadGrid>>();
        foreach (var secondLevelList in original)
        {
            var secondLevelCopiedList = new List<PadGrid>();
            foreach (var padGrid in secondLevelList)
            {
                var newPadGrid = new PadGrid(padGrid);
                secondLevelCopiedList.Add(newPadGrid);
            }
            deepCopiedList.Add(secondLevelCopiedList);
        }
        return deepCopiedList;
    }
    public static List<List<Chess>> DeepCopy(this List<List<Chess>> original)
    {
        if (original == null)
        {
            return null;
        }
        var deepCopiedList = new List<List<Chess>>();
        foreach (var secondLevelList in original)
        {
            var secondLevelCopiedList = new List<Chess>();
            foreach (var chess in secondLevelList)
            {
                if (chess == null)
                {
                    secondLevelCopiedList.Add(null);
                    continue;
                }
                var copiedChess = chess.Clone();
                secondLevelCopiedList.Add(copiedChess);
            }
            deepCopiedList.Add(secondLevelCopiedList);
        }
        return deepCopiedList;
    }
    public static List<List<bool>> DeepCopy(this List<List<bool>> original)
    {
        if (original == null)
        {
            return null;
        }
        var deepCopiedList = new List<List<bool>>();
        foreach (var secondLevelList in original)
        {
            var secondLevelCopiedList = new List<bool>();
            foreach (var chess in secondLevelList)
            {
                secondLevelCopiedList.Add(chess);
            }
            deepCopiedList.Add(secondLevelCopiedList);
        }
        return deepCopiedList;
    }
    public static bool Compare(List<List<int>> list1, List<List<int>> list2)
    {
        if (list1 == null || list2 == null) return false;
        for (int x = 0; x < list1.Count; x++)
        {
            for (int y = 0; y < list1[0].Count; y++)
            {
                if (list1[x][y] != list2[x][y]) return false;
            }
        }
        return true;
    }
    public static string FixLength(string input, int maxLength)
    {
        if (input.Length > maxLength)
        {
            return input.Substring(0, maxLength);
        }
        return input.PadRight(maxLength, ' ');
    }
    public static List<List<int>> Reverse(List<List<int>> list)
    {
        var newList = new List<List<int>>();
        foreach (var oldLine in list)
        {
            var newLine = new List<int>(oldLine);
            newLine.Reverse();
            newList.Add(newLine);
        }
        return newList;
    }
    public static PosStatus Reverse(PosStatus posStatus)
    {
        switch (posStatus)
        {
            case PosStatus.LEVEL_ONE_PLAYER:
            case PosStatus.LEVEL_TWO_PLAYER:
            case PosStatus.LEVEL_THREE_PLAYER:
                posStatus = (PosStatus)((int)posStatus + (int)PosStatus.EMPTY);
                break;
            case PosStatus.LEVEL_ONE_RIVAL:
            case PosStatus.LEVEL_TWO_RIVAL:
            case PosStatus.LEVEL_THREE_RIVAL:
                posStatus = (PosStatus)((int)posStatus - (int)PosStatus.EMPTY);
                break;
            case PosStatus.OCCUPIED_PLAYER:
                posStatus = PosStatus.OCCUPIED_RIVAL;
                break;
            case PosStatus.OCCUPIED_RIVAL:
                posStatus = PosStatus.OCCUPIED_PLAYER;
                break;
            default:
                break;
        }
        return posStatus;
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

    public override readonly string ToString()
    {
        return $"({x}, {y})";
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