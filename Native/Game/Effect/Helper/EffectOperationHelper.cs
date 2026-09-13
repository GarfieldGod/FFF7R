namespace FFF7RCore.Effect
{
    public static class EffectOperationHelper
    {
        public static void TriggerEffect(ChessPad chessPad, Int2D pos, bool once = true, ChessPadManager chessPadManager = null)
        {
            PadGrid padGrid = chessPad[pos];
            ChessProperty property = padGrid.Chess;
            if (padGrid.Empty || property.CardEffect == null || padGrid.Status < PosStatus.OCCUPIED_PLAYER) return;
            PlayerType playerType = padGrid.Status == PosStatus.OCCUPIED_PLAYER ? PlayerType.PLAYER : PlayerType.RIVAL;

            EffectOperation operation = property.CardEffect.Operation;
            switch (operation)
            {
                case EffectOperation.PowerUp:
                case EffectOperation.PowerDown:
                    PowerChangeEffect(new Input(pos, property, playerType), chessPad, once);
                    break;
                case EffectOperation.DestroyCard:
                    // EffectTrigger.DestroyCardEffect();
                    break;
                case EffectOperation.AddCardToHand:
                    // EffectTrigger.AddCardToHandEffect();
                    break;
                case EffectOperation.IncreaseFieldLevel:
                    break;
                case EffectOperation.SpawnCardOnFieldSlot:
                    var cards = property.CardEffect.CardCodeList;
                    if (cards == null || cards.Count == 0 || chessPadManager == null) break;

                    var padGrids = chessPad.GetGrids(
                        (padGrid) => padGrid.Empty && padGrid.Status != PosStatus.EMPTY
                    );
                    foreach (var grid in padGrids)
                    {
                        int gridLevel = (int)grid.Status % 10;
                        var input = new Input(grid.Position, cards[gridLevel - 1], playerType);
                        chessPadManager.Input(input);
                    }
                    break;
                case EffectOperation.AddScore:
                    break;
                case EffectOperation.TransferScore:
                    break;
            }
        }

        private static void PowerChangeEffect(Input input, ChessPad chessPad, bool dstOwned)
        {
            Log.TestLine("PowerChangeEffect Start", TextColor.BLACK);
            // 要作用的对象坐标以及值，dstOwned表示buff是否由目标拥有，如果是则需确保目标位置是有效的
            Dictionary<Int2D, int> targets = new Dictionary<Int2D, int>();

            PlayerType srcType = input.playerType; // buff施加者类型
            EffectTarget targetType = input.chess.CardEffect.Target; // buff的作用者类型
            EffectOperation operation = input.chess.CardEffect.Operation; // buff操作类型
            EffectValueType effectValueType = input.chess.CardEffect.ValueType; // buff值类型
            int effectValue;

            switch (effectValueType) {
                case EffectValueType.AsKilledOne:
                    effectValue = chessPad[input.pos].LastPowerSnapshot;
                    break;
                case EffectValueType.CountBased:
                    (int playerBuffed, int playerDeBuffed, int rivalBuffed, int rivalDeBuffed) = 
                        chessPad.GetBuffedAndDeBuffedNum(new List<Int2D> { input.pos });
                    EffectCondition condition = input.chess.CardEffect.Condition;
                    int count = 0;
                    bool player = srcType == PlayerType.PLAYER;
                    if (condition == EffectCondition.BuffedFriendNum) count = player ? playerBuffed : rivalBuffed;
                    else if (condition == EffectCondition.DeBuffedFriendNum) count = player ? playerDeBuffed : rivalDeBuffed;
                    else if (condition == EffectCondition.BuffedEnemyNum) count = player ? rivalBuffed : playerBuffed;
                    else if (condition == EffectCondition.DeBuffedEnemyNum) count = player ? rivalDeBuffed : playerDeBuffed;
                    else if (condition == EffectCondition.BuffedFriendOrEnemyNum) count = playerBuffed + rivalBuffed;
                    else if (condition == EffectCondition.DeBuffedFriendOrEnemyNum) count = playerDeBuffed + rivalDeBuffed;
                    effectValue = count * input.chess.CardEffect.Value;
                    break;
                default:
                    effectValue = input.chess.CardEffect.Value;
                    break;
            };

            if (targetType == EffectTarget.Self) targets[input.pos] = effectValue;
            else  targets = dstOwned ? CardEffect.ParseCardEffectInScope(input, chessPad) : CardEffect.ParseCardEffectsInPosition(input, chessPad.Size);

            foreach (var target in targets)
            {
                Int2D dstPos = target.Key;
                int value = effectValue;
                string mark = null;

                if (effectValueType == EffectValueType.CountBased)
                {
                    mark = "CountBased";
                    chessPad[dstPos].BuffList.RemoveAll(buff => buff.mark == mark);
                }

                if (operation == EffectOperation.PowerUp) value = effectValue;
                else if (operation == EffectOperation.PowerDown) value = -effectValue;

                Buff buff = new Buff(dstOwned ? dstPos : input.pos, input.pos, value, targetType, srcType, mark);
                Log.TestLine($"Add buff: value: {buff.value} src: {buff.source} dst: {dstPos} owner: {buff.owner} srcType: {buff.srcType} dstType: {buff.dstType} mark: {buff.mark}", 
                    TextColor.BLACK);
                chessPad.AddBuff(dstPos, buff);
            }
        }
    }
}