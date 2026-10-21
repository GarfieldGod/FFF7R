namespace FFF7R
{
    /// <summary>
    /// 对局席位。对应内核的 PlayerType，但只有两个真正参与对局的值加一个“无”。
    /// </summary>
    public enum PlayerSeat
    {
        /// <summary>无主：空格子、平局、来源不明。</summary>
        None = 0,
        Player = 1,
        Rival = 2,
    }
}
