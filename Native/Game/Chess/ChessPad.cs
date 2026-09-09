using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using Test;

public class ChessPad
{
    public Int2D Size => size_;
    public int Height => padGrids_.Count;
    public int Width { get {return padGrids_.Count > 0 ? padGrids_[0].Count : 0;}}
    public List<List<PadGrid>> GridMap => padGrids_;

    private readonly Int2D size_;
    private List<List<PadGrid>> padGrids_ = new List<List<PadGrid>> { };

    public ChessPad(int x, int y)
    {
        size_ = new Int2D(x, y);
        InitGridMap(x, y);
    }

    public ChessPad(Int2D size)
    {
        size_ = size;
        InitGridMap(size.x, size.y);
    }

    public ChessPad(List<List<PadGrid>> padGrids)
    {
        size_ = new Int2D(padGrids.Count, padGrids[0].Count);
        padGrids_ = padGrids;
    }

    // 深拷贝
    public ChessPad(ChessPad chessPad)
    {
        size_ = chessPad.Size;
        padGrids_ = Utils.DeepCopy(chessPad.GridMap);
    }

    public ChessPad DeepCopy()
    {
        var result = new ChessPad(Utils.DeepCopy(GridMap));
        return result;
    }

    public PadGrid this[int x, int y]
    {
        get
        {
            return padGrids_[x][y];
        }
    }

    public PadGrid this[Int2D pos]
    {
        get
        {
            return padGrids_[pos.x][pos.y];
        }
    }

    public List<List<int>> StatusMap
    {
        get{
            List<List<int>> result = new List<List<int>>();
            for (int x = 0; x < padGrids_.Count; x++)
            {
                var line = new List<int>();
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    line.Add((int)padGrids_[x][y].Status);
                }
                result.Add(line);
            }
            return result;
        }
        set
        {
            for (int x = 0; x < padGrids_.Count; x++)
            {
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    padGrids_[x][y].Status = (PosStatus)value[x][y];
                }
            }
        }
    }

    public List<List<Chess>> ChessMap
    {
        get {
            List<List<Chess>> result = new List<List<Chess>>();
            for (int x = 0; x < padGrids_.Count; x++)
            {
                var line = new List<Chess>();
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    if (padGrids_[x][y].Chess != null)
                    {
                        line.Add(new Chess(padGrids_[x][y].Chess));
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
    }

    public List<List<BuffList>> BuffsMap
    {
        get 
        {
            List<List<BuffList>> result = new List<List<BuffList>>();
            for (int x = 0; x < padGrids_.Count; x++)
            {
                var line = new List<BuffList>();
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    line.Add(padGrids_[x][y].BuffList);
                }
                result.Add(line);
            }
            return result;
        }
        set
        {
            for (int x = 0; x < padGrids_.Count; x++)
            {
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    padGrids_[x][y].BuffList = value[x][y];
                }
            }
        }
    }

    public List<List<bool>> BuffStatus
    {
        get
        {
            List<List<bool>> result = new List<List<bool>>();
            for (int x = 0; x < padGrids_.Count; x++)
            {
                var line = new List<bool>();
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    line.Add(padGrids_[x][y].NeverBuffed);
                }
                result.Add(line);
            }
            return result;
        }
        set
        {
            for (int x = 0; x < padGrids_.Count; x++)
            {
                for (int y = 0; y < padGrids_[0].Count; y++)
                {
                    padGrids_[x][y].NeverBuffed = value[x][y];
                }
            }
        }
    }

    public void SetChess(Int2D pos, Chess src)
    {
        if (src != null)
        {
            padGrids_[pos.x][pos.y].Chess = src.Property;
        }
        else
        {
            padGrids_[pos.x][pos.y].Chess = null;
        }
    }

    public List<List<int>> CardLevelMap
    {
        get
        {
            List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
            for (int x = 0; x < stayBuffMapResult.Count; x++)
            {
                for (int y = 0; y < stayBuffMapResult[0].Count; y++)
                {
                    stayBuffMapResult[x][y] = padGrids_[x][y].Level;
                }
            }
            return stayBuffMapResult;
        }
    }

    public List<List<int>> ScoreMap
    {
        get
        {
            List<List<int>> ret = new List<List<int>>();
            for (int x = 0; x < CardLevelMap.Count; x++)
            {
                int playerScore = 0;
                int rivalScore = 0;
                for (int y = 0; y < CardLevelMap[0].Count; y++)
                {
                    PosStatus posStatus = (PosStatus)StatusMap[x][y];
                    if (posStatus == PosStatus.OCCUPIED_PLAYER)
                    {
                        playerScore += CardLevelMap[x][y];
                    }
                    else if (posStatus == PosStatus.OCCUPIED_RIVAL)
                    {
                        rivalScore += CardLevelMap[x][y];
                    }
                }
                ret.Add(new List<int>() { playerScore, rivalScore });
            }
            return ret;
        }
    }

    public bool AddBuff(Int2D pos, Buff buff, PlayerType playerType, bool CheckIfFirstBuffed = true)
    {
        PadGrid padGrid = padGrids_[pos.x][pos.y];
        padGrid.AddBuff(buff);
        if (CheckIfFirstBuffed) CheckFirstBuffed(padGrid);
        return false;
    }

    public void RestPos(Int2D pos)
    {
        RemoveBuffs(pos);
        padGrids_[pos.x][pos.y].Reset();
    }

    public void CheckFirstBuffed(PadGrid padGrid) { 
        if (!padGrid.Empty)
        {   
            int deBuffValue = padGrid.DeBuffValue;
            int buffValue = padGrid.BuffValue;
            bool neverBuffed = padGrid.NeverBuffed;
            bool neverDeBuffed = padGrid.NeverDeBuffed;
            if(buffValue != 0) padGrid.NeverBuffed = false;
            if(deBuffValue != 0) padGrid.NeverDeBuffed = false;

            ChessProperty property = padGrid.Chess;
            if (property.CardEffects == null) return;
            if (property.CardEffects.Item2 == EffectCondition.First_Buffed && buffValue > 0 && neverBuffed)
            {
                Log.TestLine("First_Buffed", TextColor.PURPLE);
                // EffectTrigger(padGrid.Position);
            } else if (property.CardEffects.Item2 == EffectCondition.First_DeBuffed && deBuffValue < 0 && neverDeBuffed){ 
                Log.TestLine("First_DeBuffed", TextColor.PURPLE);
                // EffectTrigger(padGrid.Position);
            }
        }
    }

    public void CheckDead()
    {
        bool hasDead = false;
        foreach (var line in padGrids_)
        {
            foreach (var padGrid in line)
            {
                if (!padGrid.Empty && padGrid.Level <= 0)
                {
                    hasDead = true;
                    // Log.TestLine("Dead: " + padGrid.GetID() + " Name: " + padGrid.GetChess().Name, TextColor.PURPLE);
                    DeadEffect(padGrid);
                    RestPos(padGrid.Position);
                }
            }
        }
        if (hasDead) CheckDead();
    }

    public void DeadEffect(PadGrid padGrid) {
        ChessProperty property = padGrid.Chess;
        if (property.CardEffects == null) return;
        if (property.CardEffects.Item2 == EffectCondition.ON_SELF_DEAD)
        {
            // Log.TestLine(padGrid.GetID() + " ON_SELF_DEAD", TextColor.PURPLE);
            // EffectTrigger(padGrid.Position);
        }
    }

    public bool RemoveBuffs(Int2D pos)
    {
        // Log.TestLine("---RemoveBuffs--- " + id);
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[x].Count; y++)
            {
                var buffs = padGrids_[x][y].BuffList;
                buffs.Remove(pos);
            }
        }
        return true;
    }

    public void InitStandard()
    {
        InitGridMap(3, 5);
        var chessGridStatus = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 }
        };
        StatusMap = chessGridStatus;
    }

    public void InitGridMap(int height, int lengh)
    {
        List<List<PadGrid>> padGrids = new List<List<PadGrid>> { };
        for (int x = 0; x < height; x++)
        {
            List<PadGrid> line = new List<PadGrid>();
            for (int y = 0; y < lengh; y++)
            {
                PadGrid padGrid = new PadGrid(new Int2D(x, y));
                // padGridsposMap_.Add(new Int2D(x, y), padGrid);
                line.Add(padGrid);
            }
            padGrids.Add(line);
        }
        padGrids_ = padGrids;
    }
}