namespace FFF7R
{
    /// <summary>
    /// 对局阶段。内核的 INIT/DISPENSE/REDISPENSE/Ready/GAMING/GAME_END/COMPUTE_RESULT/GAME_OVER
    /// 被收敛成 6 个：GAME_END、COMPUTE_RESULT、GAME_OVER 对表现层没有区别，统一为 <see cref="GameOver"/>。
    /// </summary>
    public enum MatchStatus
    {
        Init = 0,
        Dispense = 1,
        ReDispense = 2,
        Ready = 3,
        Gaming = 4,
        GameOver = 5,
    }
}
