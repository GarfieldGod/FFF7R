public class Log {
    public static void test (string info) {
    #if UNITY_ENGINE
        Debug.Log(info);
    #else
        Console.WriteLine(info);
    #endif
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

    public override bool Equals(object obj)
    {
        if (obj is Int2D)
        {
            Int2D other = (Int2D)obj;
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
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

    public override bool Equals(object obj)
    {
        if (obj is Int3D)
        {
            Int3D other = (Int3D)obj;
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
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

    public override bool Equals(object obj)
    {
        if (obj is Float3D)
        {
            Float3D other = (Float3D)obj;
            return this == other;
        }
        return false;
    }
}