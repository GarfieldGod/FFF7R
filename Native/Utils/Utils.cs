namespace FFF7RCore {
    using Effect;
    using System.Collections.Generic;
    using System.Linq;

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
        public static List<List<int>> DeepCopy(List<List<int>> original)
        {
            if (original == null) return null;
            List<List<int>> result = original.Select(innerList => new List<int>(innerList)).ToList();
            return result;
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
        public static Dictionary<Int2D, int> ReverseEffectOffset(Dictionary<Int2D, int> dict)
        {
            var newDict = new Dictionary<Int2D, int>();
            foreach (var kvp in dict)
            {
                Int2D newKey = new Int2D(kvp.Key.x, -kvp.Key.y);
                newDict[newKey] = kvp.Value;
            }
            return newDict;
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

        // 用于表达“位置未知”，避免各处各自写 (-1, -1)
        public static readonly Int2D Unknown = new Int2D(-1, -1);

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
}
