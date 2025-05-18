using System.Drawing;

public class ChessPad
{
    private Int2D size_;
    protected List<List<PadGrid>> padGrids_ = new List<List<PadGrid>> { };
    public ChessPad(int x, int y)
    {
        size_ = new Int2D(x, y);
        padGrids_ = InitGridMap(x, y);
    }
    public ChessPad(Int2D size)
    {
        size_ = new Int2D(size.x, size.y);
        padGrids_ = InitGridMap(size.x, size.y);
    }
    public ChessPad(List<List<PadGrid>> padGrids)
    {
        size_ = new Int2D(padGrids.Count, padGrids[0].Count);
        padGrids_ = padGrids;
    }
    public Int2D GetSize()
    {
        return size_;
    }
    public ChessPad DeepCopy()
    {
        var result = new ChessPad(GetSize().x, GetSize().y);
        result.SetGridStatusMap(Utils.DeepCopy2DList(GetGridStatusMap()));
        result.SetChessMap(Utils.DeepCopy(GetChessMap()));
        result.SetBuffsMap(Utils.DeepCopy(GetBuffsMap()));
        result.SetGridBackUpMap(Utils.DeepCopy2DList(GetGridBackUp()));
        result.SetBuffStatus(Utils.DeepCopy(GetBuffStatus()));
        return result;
    }
    public static ChessPad Reverse(ChessPad originalChessPad)
    {
        var result = new ChessPad(originalChessPad.GetSize().x, originalChessPad.GetSize().y);
        result.SetGridStatusMap(Rival.GetChessPosStatusInRivalView(originalChessPad.GetGridStatusMap()));
        result.SetChessMap(Rival.GetChessMapInRivalView(originalChessPad.GetChessMap()));
        result.SetBuffsMap(Rival.GetStayBuffMapInRivalView(originalChessPad.GetBuffsMap()));
        result.SetGridBackUpMap(Rival.GetChessPosStatusInRivalView(originalChessPad.GetGridBackUp()));
        result.SetBuffStatus(Rival.GetBuffStatusInRivalView(originalChessPad.GetBuffStatus()));
        return result;
    }
    public List<List<PadGrid>> GetGridMap()
    {
        return padGrids_;
    }
    public List<List<int>> GetGridStatusMap()
    {
        List<List<int>> result = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                result[x][y] = (int)padGrids_[x][y].GetGridStatus();
            }
        }
        return result;
    }
    public void SetGridStatusMap(List<List<int>> src)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                padGrids_[x][y].SetGridStatus((ChessPosStatus)src[x][y]);
            }
        }
    }
    public void SetGridBackUpMap(List<List<int>> src)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                padGrids_[x][y].SetGridBackUp((ChessPosStatus)src[x][y]);
            }
        }
    }
    public List<List<Chess>> GetChessMap()
    {
        List<List<Chess>> result = new List<List<Chess>>();
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = new List<Chess>();
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                if (padGrids_[x][y].GetChess() != null)
                {
                    line.Add(new Chess(padGrids_[x][y].GetChess()));
                }
                else
                {
                    line.Add(null);
                }
            }
            result.Add(line);
        }
        return result;
    }
    public void SetChessMap(List<List<Chess>> src)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                if (src[x][y] != null)
                {
                    padGrids_[x][y].SetChess(src[x][y].GetChessProperty());
                }
                else
                {
                    padGrids_[x][y].SetChess(null);
                }
            }
        }
    }
    public List<List<int>> GetCardLevelMapInInputerType(InputerType inputerType)
    {
        List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
        for (int x = 0; x < stayBuffMapResult.Count; x++)
        {
            for (int y = 0; y < stayBuffMapResult[0].Count; y++)
            {
                stayBuffMapResult[x][y] = Buff.Compute(padGrids_[x][y].GetBuffList(), inputerType);
            }
        }
        return stayBuffMapResult;
    }
    public List<List<int>> GetCardLevelResult()
    {
        List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
        for (int x = 0; x < stayBuffMapResult.Count; x++)
        {
            for (int y = 0; y < stayBuffMapResult[0].Count; y++)
            {
                // Log.TestLine("x : "+ x +" y: "+ y + " buffCount: " + stayBuffMap_[x][y].Count);
                ChessPosStatus chessPosStatus = padGrids_[x][y].GetGridStatus();
                int buffValue = 0;
                switch (chessPosStatus)
                {
                    case ChessPosStatus.OCCUPIED_FRIEND: buffValue = Buff.Compute(padGrids_[x][y].GetBuffList(), InputerType.PLAYER); break;
                    case ChessPosStatus.OCCUPIED_ENEMY: buffValue = Buff.Compute(padGrids_[x][y].GetBuffList(), InputerType.RIVAL); break;
                }
                stayBuffMapResult[x][y] = buffValue;
            }
        }
        return stayBuffMapResult;
    }
    public List<List<List<Buff>>> GetBuffsMap()
    {
        List<List<List<Buff>>> result = new List<List<List<Buff>>>();
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = new List<List<Buff>>();
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                line.Add(padGrids_[x][y].GetBuffList());
            }
            result.Add(line);
        }
        return result;
    }
    public void SetBuffsMap(List<List<List<Buff>>> buffs)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                padGrids_[x][y].SetBuffList(buffs[x][y]);
            }
        }
    }
    public List<List<int>> GetGridBackUp()
    {
        List<List<int>> result = new List<List<int>>();
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = new List<int>();
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                line.Add((int)padGrids_[x][y].GetGridBackUp());
            }
            result.Add(line);
        }
        return result;
    }
    public List<List<bool>> GetBuffStatus()
    {
        List<List<bool>> result = new List<List<bool>>();
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = new List<bool>();
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                line.Add(padGrids_[x][y].GetBuffStatus());
            }
            result.Add(line);
        }
        return result;
    }
    public void SetBuffStatus(List<List<bool>> buffs)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                padGrids_[x][y].SetBuffStatus(buffs[x][y]);
            }
        }
    }
    public bool AddBuff(Int2D pos, Buff buff)
    {
        padGrids_[pos.x][pos.y].AddBuff(buff);
        return true;
    }
    public void RestPos(Int2D pos)
    {
        RemoveBuffs(padGrids_[pos.x][pos.y].GetID());
        padGrids_[pos.x][pos.y].Reset();
    }
    public void RestPos(List<Int2D> pos)
    {
        foreach (var p in pos)
        {
            RestPos(p);
        }
    }
    public bool RemoveBuffs(string id)
    {
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = padGrids_[x];
            for (int y = 0; y < line.Count; y++)
            {
                var buffs = line[y].GetBuffList();
                for (int z = 0; z < buffs.Count; z++)
                {
                    if (buffs[z].id == id)
                    {
                        buffs.Remove(buffs[z]);
                    }
                }
            }
        }
        return true;
    }

    public void Copy(ChessPad chessPad)
    {
        if (chessPad == null) return;
        padGrids_ = chessPad.GetGridMap();
        size_ = chessPad.GetSize();
    }
    public void InitStandard()
    {
        padGrids_ = InitGridMap(3, 5);
        var chessGridStatus = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 }
        };
        SetGridStatusMap(chessGridStatus);
        SetGridBackUpMap(chessGridStatus);
    }
    public static List<List<PadGrid>> InitGridMap(int height, int lengh)
    {
        List<List<PadGrid>> padGrids = new List<List<PadGrid>> { };
        for (int x = 0; x < height; x++)
        {
            List<PadGrid> line = new List<PadGrid>();
            for (int y = 0; y < lengh; y++)
            {
                PadGrid padGrid = new PadGrid();
                line.Add(padGrid);
            }
            padGrids.Add(line);
        }
        return padGrids;
    }
}