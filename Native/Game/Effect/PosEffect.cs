static class PosEffect {
    public static List<List<int>> DoPosEffect(Int2D gridPos, List<List<int>> posEffects, List<List<int>> posStatus, PlayerType playerType) {
        List<List<int>> posStatusTemp = Utils.DeepCopy2DList(posStatus);

        bool ifPlayer = playerType == PlayerType.PLAYER;

        posStatusTemp[gridPos.x][gridPos.y] = ifPlayer ? (int)PosStatus.OCCUPIED_PLAYER : (int)PosStatus.OCCUPIED_RIVAL;
        posEffects = ifPlayer ? posEffects : Utils.Reverse(posEffects);

        Int2D padSize = new Int2D(posStatusTemp.Count, posStatusTemp[0].Count);
        var relativeTask = EffectsParser.ParseEffectsInRelative(posEffects, true);
        var effectTask = EffectsParser.ParseEffectsInPosition(padSize, gridPos, relativeTask);

        return ExecutePosEffect(effectTask, posStatusTemp, playerType);
    }

    private static List<List<int>> ExecutePosEffect(List<Tuple<Int2D, int>> effectTask, List<List<int>> gridStatus, PlayerType playerType) {
        // Log.TestLine("effectTask: " + effectTask.Count, TextColor.PURPLE);
        foreach (Tuple<Int2D, int> task in effectTask)
        {
            // Log.TestLine("task: x: " + task.Item1.x + " y: " + task.Item1.y + " value: " + task.Item2, TextColor.PURPLE);
            int pastLevel = gridStatus[task.Item1.x][task.Item1.y];
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
                    newLevel += task.Item2;
                }
            }
            else if (playerType == PlayerType.RIVAL)
            {
                if (pastLevel >= (int)PosStatus.EMPTY)
                {
                    newLevel += task.Item2;
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
            gridStatus[task.Item1.x][task.Item1.y] = newLevel;
            // Log.TestLine("pastLevel: " + pastLevel + " newLevel: " + newLevel, TextColor.PURPLE);
        }
        return gridStatus;
    }
}