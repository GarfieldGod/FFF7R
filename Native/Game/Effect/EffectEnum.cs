namespace FFF7RCore.Effect {
    public enum EffectCondition {
        OnPlayed,                   // 配置时

        OnSelfDead,                 // 被消灭时

        OnPosition,                 // 此卡片配置中

        OnFirstBuffed,              // 首次被强化时
        OnFirstDeBuffed,            // 首次被弱化时
        OnFirstBuffedOrDeBuffed,    // 首次被强化或弱化时

        OnBuffed,                   // 被强化时 
        OnDeBuffed,                 // 被弱化时
        OnBuffedOrDeBuffed,         // 被强化或弱化时

        OnFriendDead,               // 我方卡片被消灭时
        OnEnemyDead,                // 对方卡片被消灭时
        OnFriendOrEnemyDead,        // 我方·对方卡片被消灭时

        BuffedFriendNum,            // 我方被强化数量
        BuffedEnemyNum,             // 对方被强化数量
        BuffedFriendOrEnemyNum,     // 我方·对方被强化数量
        DeBuffedFriendNum,          // 我方被弱化数量
        DeBuffedEnemyNum,           // 对方被弱化数量
        DeBuffedFriendOrEnemyNum,   // 我方·对方被弱化数量

        OnFriendPlayed,             // 我方卡片被配置时
        OnEnemyPlayed,              // 对方卡片被配置时
        OnFriendOrEnemyPlayed,      // 我方·对方卡片被配置时

        OnPowerFirstReach,          // 威力首次达到

        OnTurnEnd,                  // 回合结束时

        OnLineWin                   // 行胜利时
    }

    public enum EffectTarget {
        FriendOnly,             // 我方
        EnemyOnly,              // 对手
        FriendAndEnemy,         // 我方·对手
        Self,                   // 自身
        Hand,                   // 手牌
        FieldSlot,              // 阵地
        ScoreCounter            // 积分
    }

    public enum EffectOperation
    {
        PowerUp,                // 威力提升
        PowerDown,              // 威力降低
        DestroyCard,            // 消灭卡片
        AddCardToHand,          // 手牌增加卡牌
        IncreaseFieldLevel,     // 阵地等级提升
        SpawnCardOnFieldSlot,   // 在阵地生成卡片
        SpawnFieldSlots,        // 生成阵地
        AddScore,               // 增加积分
        TransferScore           // 转移分数
    }

    public enum EffectValueType {
        Const,                  // 常数
        CardCode,               // 卡片代码
        AsKilledOne,            // 依据被消灭的卡片
        LineRivalScore,         // 依据对手的行分数
        CountBased,             // 依据数量
        FieldSlotLevel          // 依据阵地等级
    }

    public enum PosStatus {
        LEVEL_ONE_PLAYER = 1,
        LEVEL_TWO_PLAYER = 2,
        LEVEL_THREE_PLAYER = 3,
        EMPTY = 10,
        LEVEL_ONE_RIVAL = 11,
        LEVEL_TWO_RIVAL = 12,
        LEVEL_THREE_RIVAL = 13,
        OCCUPIED_PLAYER = 14,
        OCCUPIED_RIVAL = 15
    }

    public enum EffectType
    {
        Grid,
        Card
    }
}