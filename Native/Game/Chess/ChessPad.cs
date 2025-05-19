using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using Test;

public class ChessPad
{
    private Int2D size_;
    private List<List<PadGrid>> padGrids_ = new List<List<PadGrid>> { };
    private Dictionary<Int2D, PadGrid> padGridsposMap_ = new Dictionary<Int2D, PadGrid>();
    public ChessPad(int x, int y)
    {
        size_ = new Int2D(x, y);
        InitGridMap(x, y);
    }
    public ChessPad(Int2D size)
    {
        size_ = new Int2D(size.x, size.y);
        InitGridMap(size.x, size.y);
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
        var result = new ChessPad(Utils.DeepCopy(GetGridMap()));
        return result;
    }
    public static ChessPad Reverse(ChessPad originalChessPad)
    {
        var temp = originalChessPad.DeepCopy();
        var result = new List<List<PadGrid>>();
        foreach (var line in temp.GetGridMap())
        {
            line.Reverse();
            for(int i = 0; i < line.Count; i++)
            {
                line[i] = line[i].Reverse();
            }
            result.Add(line);
        }
        return new ChessPad(result);
    }
    public List<List<PadGrid>> GetGridMap()
    {
        return padGrids_;
    }
    public List<List<int>> GetGridStatusMap()
    {
        List<List<int>> result = new List<List<int>>();
        for (int x = 0; x < padGrids_.Count; x++)
        {
            var line = new List<int>();
            for (int y = 0; y < padGrids_[0].Count; y++)
            {
                line.Add((int)padGrids_[x][y].GetGridStatus());
            }
            result.Add(line);
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
    public void SetChess(Int2D pos, Chess src)
    {
        if (src != null)
        {
            padGrids_[pos.x][pos.y].SetChess(src.GetChessProperty());
        }
        else
        {
            padGrids_[pos.x][pos.y].SetChess(null);
        }
    }
    public List<List<int>> GetCardLevelMapInInputerType(InputerType inputerType)
    {
        List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(padGrids_[0].Count, padGrids_.Count);
        for (int x = 0; x < stayBuffMapResult.Count; x++)
        {
            for (int y = 0; y < stayBuffMapResult[0].Count; y++)
            {
                stayBuffMapResult[x][y] = Buff.Compute(padGrids_[x][y].GetBuffList(), inputerType == InputerType.PLAYER ? ChessPosStatus.OCCUPIED_FRIEND : ChessPosStatus.OCCUPIED_ENEMY, new Int2D(-1, -1));
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
                ChessPosStatus chessPosStatus = padGrids_[x][y].GetGridStatus();
                int buffValue = Buff.Compute(padGrids_[x][y].GetBuffList(), chessPosStatus, new Int2D(-1, -1));
                // Log.TestLine("x : "+ x +" y: "+ y + " buffValue: " + buffValue);
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
    public bool AddBuff(Int2D pos, Buff buff, InputerType inputerType)
    {
        PadGrid padGrid = padGrids_[pos.x][pos.y];
        padGrid.AddBuff(buff);
        CheckFristBuffed(padGrid);
        return false;
    }
    public void RestPos(Int2D pos)
    {
        RemoveBuffs(padGrids_[pos.x][pos.y].GetID());
        padGrids_[pos.x][pos.y].Reset();
    }
    public void CheckFristBuffed(PadGrid padGrid) { 
        if (!padGrid.Empty())
        {   
            int deBuffValue = padGrid.GetDebuffValue();
            int buffValue = padGrid.GetBuffValue();
            bool neverBuffed = padGrid.GetBuffStatus();
            bool neverDeBuffed = padGrid.GetDeBuffStatus();
            if(buffValue != 0) padGrid.SetBuffStatus(false);
            if(deBuffValue != 0) padGrid.SetDeBuffStatus(false);

            ChessProperty property = padGrid.GetChess();
            if (property.CardEffects == null) return;
            if (property.CardEffects.Item2 == EffectCondition.Frist_Buffed && buffValue > 0 && neverBuffed)
            {
                Log.TestLine("Frist_Buffed", TextColor.PURPLE);
                EffectTrrigger(padGrid.GetPos());
            } else if (property.CardEffects.Item2 == EffectCondition.Frist_Debuffed && deBuffValue < 0 && neverDeBuffed){ 
                Log.TestLine("Frist_Debuffed", TextColor.PURPLE);
                EffectTrrigger(padGrid.GetPos());
            }
        }
    }
    public void EffectTrrigger(Int2D src)
    {
        Log.TestLine("EffectTrrigger On", TextColor.BLACK);
        List<List<PadGrid>> gridMap = GetGridMap();
        ChessPosStatus status = gridMap[src.x][src.y].GetGridStatus();
        InputerType inputerType = status == ChessPosStatus.OCCUPIED_FRIEND ? InputerType.PLAYER : InputerType.RIVAL;
        ChessProperty property = gridMap[src.x][src.y].GetChess();
        if (property == null) return;
        EffectScope scope = property.CardEffects.Item1;
        EffectCondition condition = property.CardEffects.Item2;
        Input input = new Input(src, property);
        List<Tuple<Int2D, int>> tasks = CardEffect.ParseCardEffect(input, this);
        foreach (var dst in tasks)
        {
            if (!gridMap[dst.Item1.x][dst.Item1.y].Empty())
            {
                string id = gridMap[dst.Item1.x][dst.Item1.y].GetChess().Name + "_X_" + dst.Item1.x.ToString() + "_Y_" + dst.Item1.y.ToString();
                int value = dst.Item2;
                Buff buff = new Buff(src, id, value, scope, inputerType);
                AddBuff(dst.Item1, buff, inputerType);
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
                if (!padGrid.Empty() && padGrid.GetLevel() <= 0)
                {
                    hasDead = true;
                    Log.TestLine("Dead: " + padGrid.GetID() + " Name: " + padGrid.GetChess().Name, TextColor.PURPLE);
                    DeadEffect(padGrid);
                    RestPos(padGrid.GetPos());
                }
            }
        }
        if (hasDead) CheckDead();
    }
    public void DeadEffect(PadGrid padGrid) {
        ChessProperty property = padGrid.GetChess();
        if (property.CardEffects == null) return;
        if (property.CardEffects.Item2 == EffectCondition.ON_SELF_DEAD)
        {
            Log.TestLine(padGrid.GetID() + " ON_SELF_DEAD", TextColor.PURPLE);
            EffectTrrigger(padGrid.GetPos());
        }
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
        // Log.TestLine("---RemoveBuffs--- " + id);
        for (int x = 0; x < padGrids_.Count; x++)
        {
            for (int y = 0; y < padGrids_[x].Count; y++)
            {
                var buffs = padGrids_[x][y].GetBuffList();
                List<Buff> buffsToRemove = new List<Buff>();
                for (int z = 0; z < buffs.Count; z++)
                {
                    // Log.TestLine("FoundBuffs: " + buffs[z].id + " value: " + buffs[z].value);
                    if (buffs[z].id == id)
                    {
                        // Log.TestLine("__RemoveBuffs: " + buffs[z].id + " value: " + buffs[z].value);
                        buffsToRemove.Add(buffs[z]);
                    }
                }
                foreach (var buff in buffsToRemove)
                {
                    buffs.Remove(buff);
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
        InitGridMap(3, 5);
        var chessGridStatus = new List<List<int>>{
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 },
            new List<int> { 1, 10, 10, 10, 11 }
        };
        SetGridStatusMap(chessGridStatus);
        SetGridBackUpMap(chessGridStatus);
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