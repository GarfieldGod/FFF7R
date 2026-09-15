namespace FFF7RCore.Effect
{
    public static class EffectConditionHelper
    {
        public static bool HasCardEffect(ChessPad chessPad, Int2D position) {
            if (chessPad.IsInBoard(position)) return false;
            if (chessPad[position].Empty || chessPad[position].Chess.CardEffect == null) return false;
            return true;
        }

        public static bool CheckFirstBuffedOrDeBuffed(ChessPad chessPad, Int2D position) {
            if (!HasCardEffect(chessPad, position)) return false;

            PadGrid padGrid = chessPad[position];
            ChessProperty property = padGrid.Chess;

            bool triggerFirstBuffed = property.CardEffect.Condition == EffectCondition.OnFirstBuffed && padGrid.BuffValue > 0 && padGrid.NeverBuffed;
            bool triggerFirstDeBuffed = property.CardEffect.Condition == EffectCondition.OnFirstDeBuffed && padGrid.BuffValue < 0 && padGrid.NeverDeBuffed;
            return triggerFirstBuffed || triggerFirstDeBuffed;
        }

        public static EffectConditionType GetConditionType(EffectCondition condition)
        {
            switch (condition)
            {
                case EffectCondition.OnPlayed:
                case EffectCondition.OnPosition:
                case EffectCondition.OnFriendPlayed:
                case EffectCondition.OnEnemyPlayed:
                case EffectCondition.OnFriendOrEnemyPlayed:
                    return EffectConditionType.Placed;
                case EffectCondition.OnSelfDead:
                case EffectCondition.OnFriendDead:
                case EffectCondition.OnEnemyDead:
                case EffectCondition.OnFriendOrEnemyDead:
                    return EffectConditionType.Dead;
                case EffectCondition.OnFirstBuffed:
                case EffectCondition.OnFirstDeBuffed:
                case EffectCondition.OnFirstBuffedOrDeBuffed:
                case EffectCondition.OnBuffed:
                case EffectCondition.OnDeBuffed:
                case EffectCondition.OnBuffedOrDeBuffed:
                case EffectCondition.BuffedFriendNum:
                case EffectCondition.BuffedEnemyNum:
                case EffectCondition.BuffedFriendOrEnemyNum:
                case EffectCondition.DeBuffedFriendNum:
                case EffectCondition.DeBuffedEnemyNum:
                case EffectCondition.DeBuffedFriendOrEnemyNum:
                case EffectCondition.OnPowerFirstReach:
                    return EffectConditionType.Buffed;
                case EffectCondition.OnTurnEnd:
                    return EffectConditionType.Undefined;
                case EffectCondition.OnLineWin:
                    return EffectConditionType.Undefined;
                default:
                    return EffectConditionType.Undefined;
            }
        }
    }
}