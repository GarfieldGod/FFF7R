namespace FFF7RCore {
    using System.Collections;
    using System.Collections.Generic;
    using Effect;

    public class ChessPad : IEnumerable<List<PadGrid>>
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

        public bool IsInBoard(Int2D pos)
        {
            return pos.x >= 0 && pos.x < Size.x && pos.y >= 0 && pos.y < Size.y;
        }

        public PadGrid this[int x, int y]
        {
            get
            {
                return padGrids_[x][y];
            }
        }

        public IEnumerator<List<PadGrid>> GetEnumerator() => padGrids_.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
                foreach (var line in padGrids_)
                {
                    var retLine = new List<int>();
                    foreach (var padGrid in line)
                    {
                        retLine.Add((int)padGrid.Status);
                    }
                    result.Add(retLine);
                }
                return result;
            }
            set
            {
                for (int x = 0; x < size_.x; x++)
                {
                    for (int y = 0; y < size_.y; y++)
                    {
                        padGrids_[x][y].Status = (PosStatus)value[x][y];
                    }
                }
            }
        }

        public List<List<int>> CardLevelMap
        {
            get
            {
                List<List<int>> stayBuffMapResult = Utils.NewEmpty2DList(size_.y, size_.x);
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

        public Score CurrentScore
        {
            get
            {
                Score ret = new Score();
                foreach (var line in padGrids_)
                {
                    int playerScore = 0;
                    int rivalScore = 0;
                    foreach(var padGrid in line)
                    {
                        PosStatus posStatus = padGrid.Status;
                        if (posStatus == PosStatus.OCCUPIED_PLAYER)
                        {
                            playerScore += padGrid.Level;
                        }
                        else if (posStatus == PosStatus.OCCUPIED_RIVAL)
                        {
                            rivalScore += padGrid.Level;
                        }
                    }
                    ret.AddLine(playerScore, rivalScore);
                }
                return ret;
            }
        }

        public (int playerBuff, int playerDeBuff, int rivalBuff, int rivalDeBuff) GetBuffedAndDeBuffedNum(List<Int2D> exclusionList = null)
        {
            int playerBuffed = 0, playerDeBuffed = 0, rivalBuffed = 0, rivalDeBuffed = 0;
            foreach(var line in padGrids_)
            {
                foreach(var padGrid in line)
                {
                    if (padGrid.Empty || (exclusionList != null && exclusionList.Contains(padGrid.Position))) continue;

                    if (padGrid.Status == PosStatus.OCCUPIED_PLAYER)
                    {
                        if (padGrid.OnBuffed) playerBuffed++;
                        else if (padGrid.OnDeBuffed) playerDeBuffed++;
                    }
                    else if (padGrid.Status == PosStatus.OCCUPIED_RIVAL)
                    {
                        if (padGrid.OnBuffed) rivalBuffed++;
                        else if (padGrid.OnDeBuffed) rivalDeBuffed++;
                    }
                }
            }
            return (playerBuffed, playerDeBuffed, rivalBuffed, rivalDeBuffed);
        }

        // 获取所有非空的、但未被占用的格子
        public List<PadGrid> GetGridsByFilter(Func<PadGrid, bool> filter = null)
        {
            var result = new List<PadGrid>();
            foreach(var line in padGrids_)
            {
                foreach(var padGrid in line)
                {
                    if (filter != null && !filter.Invoke(padGrid)) continue;
                    result.Add(padGrid);
                }
            }
            return result;
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

        public void InitGridMap(int height, int length)
        {
            List<List<PadGrid>> padGrids = new List<List<PadGrid>> { };
            for (int x = 0; x < height; x++)
            {
                List<PadGrid> line = new List<PadGrid>();
                for (int y = 0; y < length; y++)
                {
                    PadGrid padGrid = new PadGrid(new Int2D(x, y));
                    line.Add(padGrid);
                }
                padGrids.Add(line);
            }
            padGrids_ = padGrids;
        }
    }
}