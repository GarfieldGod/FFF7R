namespace FFF7RCore.Effect {
    public enum EventKind { Placed, Buffed, Dead }

    /// 一条"待触发效果"。纯数据：没有委托、没有盘引用，可复制、可 dump、可序列化。
    public readonly struct PendingEffect {
        public readonly Int2D SourcePos;           // 效果挂在哪一格（原 input.pos）
        public readonly PlayerType SourcePlayer;   // 挂的人是谁
        public readonly EffectCondition Condition; // OnFriendDead / BuffedFriendNum ...
        public readonly EventKind Watching;        // 在等哪类事件
        public readonly int Seq;                   // 入队序号，只用于观测/回放比对
        public readonly string CardCode;           // 挂它的牌，用于校验

        public PendingEffect(Int2D pos, PlayerType player, EffectCondition cond, EventKind kind, int seq, string cardCode) {
            SourcePos = pos;
            SourcePlayer = player;
            Condition = cond;
            Watching = kind;
            Seq = seq;
            CardCode = cardCode;
        }
    }
}
