using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

public class Selector
{
    public int Count => chessList_.Count;
    private List<Chess> chessList_;
    private KeyValuePair<Chess, int> previewChess_ = new KeyValuePair<Chess, int>(null, -1);

    public Selector(List<Chess> chessList) {
        chessList_ = chessList;
    }

    public Chess this[int index]
    {
        get
        {
            if (index < 0 || index >= chessList_.Count)
            {
                return null;
            }
            return chessList_[index];
        }
        set
        {
            chessList_[index] = value;
        }
    }

    public List<Chess> ChessPool {
        get
        {
            return chessList_;
        }
        set
        {
            chessList_ = value;
        }
    }

    public bool Add(Chess chess)
    {
        if (chess == null)
        {
            return false;
        }
        chessList_.Add(chess);
        return true;
    }

    public bool RemoveAt(int index) {
        if (index < 0 || index >= chessList_.Count) {
            return false;
        }
        chessList_.RemoveAt(index);
        return true;
    }

    public bool RemoveAt(List<int> indexes) {
        bool result = true;
        for(int i = 0; i < indexes.Count; i++) {
            if (!RemoveAt(indexes[i])) {
                result = false;
            }
        }
        return result;
    }

    public bool Remove(Chess chessOne) {
        bool result = chessList_.Remove(chessOne);
        return result;
    }
}