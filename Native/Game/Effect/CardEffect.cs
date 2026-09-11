namespace FFF7RCore.Effect 
{
    static class CardEffect {
        // 效果在棋盘中的实际位置
        public static Dictionary<Int2D, int> ParseCardEffectsInPosition(Input input, Int2D padSize) {
            return EffectsParser.ParseEffectsInPosition(input, padSize, EffectType.Card);
        }

        // 在棋盘中实际位置可以生效的效果
        public static Dictionary<Int2D, int> ParseCardEffectInScope(Input input, ChessPad chessPad) {
            if (input.chess.CardEffect == null) return new Dictionary<Int2D, int>();
            var targetsInPosition = ParseCardEffectsInPosition(input, chessPad.Size);

            var targets = GetEffectInScope(input, chessPad, targetsInPosition);
            return targets;
        }

        private static Dictionary<Int2D, int> GetEffectInScope(Input input, ChessPad chessPad, Dictionary<Int2D, int> targets)
        {
            var result = new Dictionary<Int2D, int>();

            EffectTarget targetType = input.chess.CardEffect.Target;
            PlayerType playerType = input.playerType;
            foreach(var target in targets) {
                if (!chessPad.IsInBoard(target.Key)) continue;

                PosStatus posStatus = chessPad[target.Key.x, target.Key.y].Status; 
                switch (targetType) {
                    case EffectTarget.FriendAndEnemy:
                        if (posStatus == PosStatus.OCCUPIED_PLAYER || posStatus == PosStatus.OCCUPIED_RIVAL) {
                            result.Add(target.Key, target.Value);
                        }
                        break;
                    case EffectTarget.FriendOnly:
                        if (playerType == PlayerType.PLAYER && posStatus == PosStatus.OCCUPIED_PLAYER || 
                            playerType == PlayerType.RIVAL && posStatus == PosStatus.OCCUPIED_RIVAL) {
                            result.Add(target.Key, target.Value);
                        }
                        break;
                    case EffectTarget.EnemyOnly:
                        if (playerType == PlayerType.PLAYER && posStatus == PosStatus.OCCUPIED_RIVAL ||
                            playerType == PlayerType.RIVAL && posStatus == PosStatus.OCCUPIED_PLAYER) {
                            result.Add(target.Key, target.Value);
                        }
                        break;
                    default:break;
                }
            }
            return result;
        }
    }
}