static class EffectsParser {
    public static List<Tuple<Int2D, int>> ParseEffectsInRelative(List<List<int>> effects, bool ignoreSelf)
    {
        var result = new List<Tuple<Int2D, int>>();
        var midPos = new Int2D((effects.Count - 1) / 2, (effects[0].Count - 1) / 2);
        for (int i = 0; i < effects.Count; i++) {
            for (int j = 0; j < effects[i].Count; j++) {
                if (effects[i][j] != 0) {
                    int posX = i - midPos.x;
                    int posY = j - midPos.y;
                    var posWithValue = new Tuple<Int2D, int>(new Int2D(posX, posY), effects[i][j]);
                    if (ignoreSelf) {
                        if (posX != 0 || posY != 0) {
                            result.Add(posWithValue);
                        }
                    } else {
                        result.Add(posWithValue);
                    }
                    // Log.TestLine($"ParseEffectsInRelative: y: {posY} x: {posX} value: {effects[i][j]}");
                }
            }
        }
        return result;
    }

    public static List<Tuple<Int2D, int>> ParseEffectsInPosition(Int2D chessPadSize, Int2D posPos, List<Tuple<Int2D, int>> parsedEffects)
    {
        List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>();
        if (posPos.x < 0 || posPos.y < 0) {
            return result;
        }
        foreach (var effect in parsedEffects) {
            int posX = posPos.x + effect.Item1.x;
            int posY = posPos.y + effect.Item1.y;
            if (posX <  chessPadSize.x && posX >= 0 && posY <  chessPadSize.y && posY >= 0) {
                // Log.TestLine($"ParseEffectsInPosition: vertical: {posY} horizontal: {posX} value: {effect.Item2}");
                var affectedPos = new Int2D(posX, posY);
                var next = new Tuple<Int2D, int>(affectedPos, effect.Item2);
                // var next = new Tuple<string, int>(GlobalScope.chessPositionNameList[posY, posX], effect.Item2); //use chessGrid name in the past
                result.Add(next);
            }
        }
        return result;
    }
}

public struct EffectConfig {
    public EffectCondition condition;
    public EffectScope scope;
    public EffectValueType valueType;
    public int value;
}

public enum EffectCondition {
    //once
    ON_PLAYED = 0,
    ON_SELF_DEAD = 1,
    Frist_Buffed = 2,
    Frist_Debuffed = 3,
    LevelFristReach7 = 4,
    //many times
    ON_POSITION = 5,
    //many times increase
    Num_All,
    Num_Friend,
    Num_Enemy,
    Dead_All,
    Dead_Friend,
    ON_ENEMY_DEAD,
    EveryTime_Buffed,
    EveryTime_Debuffed,
    FriendPlayed,
    EnemyPlayed,
    //special
    CoverInput,
    LineWin,
}

public enum EffectScope {
    DOTOALL,
    FRIEND_ONLY,
    ENEMY_ONLY,
    Self,
    FRIEND_INCREASE_ENEMY_REDUCE_ONCE,
    ScoreCounter
}

public enum EffectValueType {
    Const,
    AsKilledOne,
    LineRivalScore
}

public enum CardEffectsScope {
    DOTOALL = 0,
    FRIEND_ONLY = 1,
    ENEMY_ONLY = 2,
    FRIEND_INCREASE_ENEMY_REDUCE_ONCE,
}
public enum CardEffectsType {
    ON_PLAYED,
    ON_POSITION,
    ON_SELF_DEAD,
    ON_ENEMY_DEAD,
    ON_ENHANCED
}