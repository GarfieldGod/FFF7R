namespace FFF7R
{
    /// <summary>
    /// 事件种类。与 <see cref="GameEvent"/> 的派生类型一一对应，
    /// 表现层若只关心种类（如打点、过滤）可先看 Kind 再决定是否拆箱。
    /// </summary>
    public enum GameEventKind
    {
        None = 0,
        ChessPlaced = 1,
        ChessDead = 2,
        ChessBuffed = 3,
        BuffsExpired = 4,
        GridLevelsChanged = 5,
        TurnEnded = 6,
        GameEnded = 7,
    }
}
