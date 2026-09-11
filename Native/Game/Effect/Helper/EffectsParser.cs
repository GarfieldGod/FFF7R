namespace FFF7RCore.Effect 
{
    public static class EffectsParser {
        // 计算以效果图为中心，有效果的相对位置
        public static Dictionary<Int2D, int> ParseEffectsInRelative(List<List<int>> effects, bool ignoreSelf = true)
        {
            var result = new Dictionary<Int2D, int>();
            var midPos = new Int2D((effects.Count - 1) / 2, (effects[0].Count - 1) / 2);
            for (int i = 0; i < effects.Count; i++) {
                for (int j = 0; j < effects[i].Count; j++) {
                    int value = effects[i][j];
                    if (value != 0) {
                        int posX = i - midPos.x;
                        int posY = j - midPos.y;
                        if (ignoreSelf) {
                            if (posX != 0 || posY != 0) {
                                result.Add(new Int2D(posX, posY), value);
                            }
                        } else {
                            result.Add(new Int2D(posX, posY), value);
                        }
                        // Log.TestLine($"ParseEffectsInRelative: y: {posY} x: {posX} value: {effects[i][j]}");
                    }
                }
            }
            return result;
        }

        // 计算效果位置在棋盘上的实际位置

        public static Dictionary<Int2D, int> ParseEffectsInPosition(Input input, Int2D padSize, EffectType type) {
            Dictionary<Int2D, int> targets = new Dictionary<Int2D, int>();

            if (input.chess == null || 
                type == EffectType.Grid && input.chess.PosEffect == null || 
                type == EffectType.Card && (input.chess.CardEffect == null || input.chess.CardEffect.Scope == null)) 
                return targets;

            var effectsMap = type == EffectType.Grid ? input.chess.PosEffect : input.chess.CardEffect.Scope;
            var offsetDict = type == EffectType.Grid ? input.chess.PosOffsetDict : input.chess.CardOffsetDict;

            if (offsetDict == null)
            {
                var temp = Utils.DeepCopy(effectsMap);
                var posEffects = input.playerType == PlayerType.PLAYER ? temp : Utils.Reverse(temp);
                var relativeTargets = ParseEffectsInRelative(posEffects);
                targets = ParseEffectsInPosition(padSize, input.pos, relativeTargets);
            } else
            {
                var temp = new Dictionary<Int2D, int>(offsetDict);
                var posTargets = input.playerType == PlayerType.PLAYER ? temp : Utils.ReverseEffectOffset(temp);
                targets = ParseEffectsInPosition(padSize, input.pos, posTargets);
            }
            return targets;
        }

        public static Dictionary<Int2D, int> ParseEffectsInPosition(Int2D chessPadSize, Int2D src, Dictionary<Int2D, int> targets)
        {
            Dictionary<Int2D, int> result = new Dictionary<Int2D, int>();
            if (src.x < 0 || src.y < 0 || src.x >= chessPadSize.x || src.y >= chessPadSize.y)
            {
                return result;
            }
            foreach (var target in targets) {
                int posX = src.x + target.Key.x;
                int posY = src.y + target.Key.y;
                if (posX < 0 || posY < 0 || posX >= chessPadSize.x || posY >= chessPadSize.y) continue;

                var affectedPos = new Int2D(posX, posY);
                result.Add(affectedPos, target.Value);
            }
            return result;
        }

        public static List<Tuple<Int2D, int>> ParseEffectsInPosition(Int2D chessPadSize, Int2D posPos, List<Tuple<Int2D, int>> parsedEffects)
        {
            List<Tuple<Int2D, int>> result = new List<Tuple<Int2D, int>>();
            if (posPos.x < 0 || posPos.y < 0)
            {
                return result;
            }
            foreach (var effect in parsedEffects) {
                int posX = posPos.x + effect.Item1.x;
                int posY = posPos.y + effect.Item1.y;
                if (posX < chessPadSize.x && posX >= 0 && posY < chessPadSize.y && posY >= 0) {
                    var affectedPos = new Int2D(posX, posY);
                    var next = new Tuple<Int2D, int>(affectedPos, effect.Item2);
                    result.Add(next);
                }
            }
            return result;
        }
    }
}