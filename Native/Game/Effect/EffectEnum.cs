namespace Effect
{
    public enum EffectCondition {
        OnPlayed,

        OnSelfDead,

        OnPosition,

        OnFirstBuffed,
        OnFirstDeBuffed,
        OnFirstBuffedOrDeBuffed,

        OnBuffed,
        OnDeBuffed,
        OnBuffedOrDeBuffed,

        OnFriendDead,
        OnEnemyDead,
        OnFriendOrEnemyDead,

        BuffedFriendNum,
        BuffedEnemyNum,
        BuffedFriendOrEnemyNum,
        DeBuffedFriendNum,
        DeBuffedEnemyNum,
        DeBuffedFriendOrEnemyNum,

        OnFriendPlayed,
        OnEnemyPlayed,
        OnFriendOrEnemyPlayed,

        OnPowerFirstReach,

        OnTurnEnd,

        OnLineWin
    }

    public enum EffectTarget {
        FriendOnly,
        EnemyOnly,
        FriendAndEnemy,
        Self,
        Hand,
        FieldSlot,
        ScoreCounter
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
        TransferScore
    }

    public enum EffectValueType {
        Const,
        AsKilledOne,
        LineRivalScore,
        CountBased,
        FieldSlotLevel
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