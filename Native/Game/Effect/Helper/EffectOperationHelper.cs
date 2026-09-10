namespace Effect
{
    public static class EffectOperationHelper
    {
        public static void TriggerEffect(ChessPad chessPad, Int2D pos)
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
                    PowerChangeOnce(new Input(pos, property, playerType), chessPad);
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
                    break;
                case EffectOperation.SpawnFieldSlots:
                    break;
                case EffectOperation.AddScore:
                    break;
                case EffectOperation.TransferScore:
                    break;
            }
        }

        public static void PowerChangeOnce(Input input, ChessPad chessPad)
        {
            PowerChangeEffect(input, chessPad, true);
        }

        public static void PowerChangeLasting(Input input, ChessPad chessPad)
        {
            PowerChangeEffect(input, chessPad, false);
        }

        private static void PowerChangeEffect(Input input, ChessPad chessPad, bool dstOwned)
        {
            Log.TestLine("PowerChangeEffect", TextColor.BLACK);

            // 要作用的对象坐标以及值，dstOwned表示buff是否由目标拥有，如果是则需确保目标位置是有效的
            Dictionary<Int2D, int> targets = 
                dstOwned ? CardEffect.ParseCardEffectInScope(input, chessPad) : CardEffect.ParseCardEffectsInPosition(input, chessPad.Size);

            PlayerType srcType = input.playerType; // buff施加者类型
            EffectTarget targetType = input.chess.CardEffect.Target; // buff的作用者类型
            EffectOperation operation = input.chess.CardEffect.Operation; // buff操作类型
            int effectValue = input.chess.CardEffect.Value; // buff值

            foreach (var target in targets)
            {
                Int2D dstPos = target.Key;
                int value = effectValue;
                if (operation == EffectOperation.PowerUp) value = effectValue;
                else if (operation == EffectOperation.PowerDown) value = -effectValue;

                Buff buff = new Buff(dstOwned ? dstPos : input.pos, input.pos, value, targetType, srcType);
                chessPad.AddBuff(dstPos, buff);
            }
        }
    }
}