namespace Effect {
    static class PosEffect {
        public static Dictionary<Int2D, int> ParsePosEffectsInPosition(Input input, Int2D padSize) {
            return EffectsParser.ParseEffectsInPosition(input, padSize, EffectType.Grid);
        }

        public static List<List<int>> DoPosEffect(Input input, ChessPad chessPad) {
            List<List<int>> posStatusTemp = Utils.DeepCopy(chessPad.StatusMap);
            if (input.chess == null || input.chess.PosEffect == null) return posStatusTemp;

            bool ifPlayer = input.playerType == PlayerType.PLAYER;
            posStatusTemp[input.pos.x][input.pos.y] = ifPlayer ? (int)PosStatus.OCCUPIED_PLAYER : (int)PosStatus.OCCUPIED_RIVAL;

            Dictionary<Int2D, int> targets = ParsePosEffectsInPosition(input, chessPad.Size);

            return ExecutePosEffect(targets, posStatusTemp, input.playerType);
        }

        private static List<List<int>> ExecutePosEffect(Dictionary<Int2D, int> targets, List<List<int>> gridStatus, PlayerType playerType) {
            // Log.TestLine("targets: " + targets.Count, TextColor.PURPLE);
            foreach (var target in targets)
            {
                // Log.TestLine("target: x: " + target.Item1.x + " y: " + target.Item1.y + " value: " + target.Item2, TextColor.PURPLE);
                int pastLevel = gridStatus[target.Key.x][target.Key.y];
                if ((PosStatus)pastLevel >= PosStatus.OCCUPIED_PLAYER)
                {
                    continue;
                }
                int newLevel = pastLevel;
                if (playerType == PlayerType.PLAYER)
                {
                    if (pastLevel > (int)PosStatus.EMPTY)
                    {
                        newLevel -= (int)PosStatus.EMPTY;
                    }
                    else
                    {
                        newLevel = pastLevel % 10;
                        newLevel += target.Value;
                    }
                }
                else if (playerType == PlayerType.RIVAL)
                {
                    if (pastLevel >= (int)PosStatus.EMPTY)
                    {
                        newLevel += target.Value;
                    }
                    else
                    {
                        newLevel += (int)PosStatus.EMPTY;
                    }
                }
                if (newLevel > (int)PosStatus.LEVEL_THREE_PLAYER && playerType == PlayerType.PLAYER)
                {
                    newLevel = (int)PosStatus.LEVEL_THREE_PLAYER;
                }
                else if (newLevel > (int)PosStatus.LEVEL_THREE_RIVAL && playerType == PlayerType.RIVAL)
                {
                    newLevel = (int)PosStatus.LEVEL_THREE_RIVAL;
                }
                gridStatus[target.Key.x][target.Key.y] = newLevel;
                // Log.TestLine("pastLevel: " + pastLevel + " newLevel: " + newLevel, TextColor.PURPLE);
            }
            return gridStatus;
        }
    }
}