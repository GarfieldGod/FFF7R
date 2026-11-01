namespace FFF7RCore {
    using Effect;
    using System.Collections.Generic;

    /// <summary>
    /// 一次落子的预览结果。所有字段都是"相对当前真盘的**最终**差异"，不含中间态。
    /// 生命周期：只在下一次 Preview/Input 之前有效，不要跨帧持有。
    /// </summary>
    public sealed class PreviewResult
    {
        public bool Valid;
        public InputReject Reject;
        public Int2D Pos;

        /// 落点在预览盘上的 Level（含全部 buff）；本卡落下即死时为 0
        public int PlacedLevel;
        /// 本卡落下后是否还留在盘上（false = 落下的瞬间就 Level <= 0 被清掉）
        public bool PlacedSurvives;

        /// 战力或地形发生变化的格子，含落点
        public List<CellChange> Cells;
        /// 除落点外消失的棋子
        public List<DeathInfo> Deaths;

        public void Clear() { Cells?.Clear(); Deaths?.Clear(); }
    }

    public readonly struct CellChange
    {
        public readonly Int2D Pos;
        public readonly int LevelBefore, LevelAfter;         // 含 buff 的战力
        public readonly PosStatus StatusBefore, StatusAfter; // 地形等级 + 归属

        public int LevelDelta => LevelAfter - LevelBefore;

        /// 这格从"有子"变成"没子"——死亡，或被落点覆盖
        public bool OccupiedToEmpty =>
            StatusBefore >= PosStatus.OCCUPIED_PLAYER && StatusAfter < PosStatus.OCCUPIED_PLAYER;

        public CellChange(Int2D pos, int levelBefore, int levelAfter, PosStatus statusBefore, PosStatus statusAfter)
        {
            Pos = pos;
            LevelBefore = levelBefore;
            LevelAfter = levelAfter;
            StatusBefore = statusBefore;
            StatusAfter = statusAfter;
        }
    }

    public readonly struct DeathInfo
    {
        public readonly Int2D Pos;
        public readonly PlayerType Owner;
        public readonly string CardCode;
        public readonly int LevelBefore;

        public DeathInfo(Int2D pos, PlayerType owner, string cardCode, int levelBefore)
        {
            Pos = pos;
            Owner = owner;
            CardCode = cardCode;
            LevelBefore = levelBefore;
        }
    }
}
