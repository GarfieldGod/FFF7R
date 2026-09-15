namespace FFF7RCore
{
    using Effect;
    public class ChessBoardEventSubscriber
    {
        private readonly EventSystem _eventSystem;
        private readonly ChessPadManager _chessPadManager;
        private readonly ChessPad _targetPad;

        private readonly Dictionary<Int2D, EventHandler<Input>> _placedHandlers = new();
        private readonly Dictionary<Int2D, EventHandler<EventSystem.ChessBuffedEventArgs>> _buffedHandlers = new();
        private readonly Dictionary<Int2D, EventHandler<EventSystem.ChessDeadEventArgs>> _deadHandlers = new();

        public ChessBoardEventSubscriber(ChessPadManager chessPadManager, EventSystem eventSystem, ChessPad targetPad)
        {
            _chessPadManager = chessPadManager;
            _eventSystem = eventSystem;
            _targetPad = targetPad;
        }

        public void UnsubscribeAllAt(Int2D pos)
        {
            UnsubscribePlaced(pos);
            UnsubscribeBuffed(pos);
            UnsubscribeDead(pos);
        }

        public void UnsubscribeAll()
        {
            foreach (var kv in _placedHandlers) _eventSystem.OnChessPlaced -= kv.Value;
            _placedHandlers.Clear();
            
            foreach (var kv in _buffedHandlers) _eventSystem.OnChessBuffed -= kv.Value;
            _buffedHandlers.Clear();
            
            foreach (var kv in _deadHandlers) _eventSystem.OnChessDead -= kv.Value;
            _deadHandlers.Clear();
        }

        #region Placed Event
        private void UnsubscribePlaced(Int2D pos)
        {
            if (_placedHandlers.TryGetValue(pos, out var h))
            {
                _eventSystem.OnChessPlaced -= h;
                _placedHandlers.Remove(pos);
            }
        }

        public void SubscribePlacedEvent(EffectCondition condition, Input input)
        {
            UnsubscribePlaced(input.pos);
            EventHandler<Input> handler = (sender, placedInput) =>
            {
                bool once = condition == EffectCondition.OnPlayed || condition == EffectCondition.OnPosition;
                bool self = input.pos == placedInput.pos;
                if (!self && once) return;
                if (self && !once) return;

                PlayerType owner = placedInput.playerType;
                bool match =
                    condition == EffectCondition.OnFriendOrEnemyPlayed
                    || (condition == EffectCondition.OnFriendPlayed &&
                        (owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER ||
                         owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
                    || (condition == EffectCondition.OnEnemyPlayed &&
                        (owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL ||
                         owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER))
                    || self && once;

                if (match)
                {
                    Log.TestLine($"Catch OnChessPlaced: {condition} pos: {input.pos} src: {placedInput.pos}");
                    EffectOperationHelper.TriggerEffect(_targetPad, input.pos, condition == EffectCondition.OnPosition ? false : true, _chessPadManager);
                }
            };
            _placedHandlers[input.pos] = handler;
            _eventSystem.OnChessPlaced += handler;
        }
        #endregion

        #region Buffed Event
        private void UnsubscribeBuffed(Int2D pos)
        {
            if (_buffedHandlers.TryGetValue(pos, out var h))
            {
                _eventSystem.OnChessBuffed -= h;
                _buffedHandlers.Remove(pos);
            }
        }

        public void SubscribeBuffedEvent(EffectCondition condition, Input input)
        {
            UnsubscribeBuffed(input.pos);
            EventHandler<EventSystem.ChessBuffedEventArgs> handler = (sender, buffedOne) =>
            {
                int value = buffedOne.Value;
                bool positive = value > 0;
                PlayerType ownerType = buffedOne.Owner;
                PlayerType selfType = input.playerType;
                bool friend = ownerType == selfType;
                Int2D pos = buffedOne.Position;

                bool match = false;
                bool isSelf = input.pos == pos;
                if (isSelf)
                {
                    bool firstPowerReach =
                        condition == EffectCondition.OnPowerFirstReach &&
                        _targetPad[input.pos].Level >= input.chess.CardEffect.PowerThreshold &&
                        _targetPad[input.pos].FirstPowerReach == false;

                    bool firstBuffed = condition == EffectCondition.OnFirstBuffed && positive &&  _targetPad[input.pos].NeverBuffed;
                    bool firstDeBuffed = condition == EffectCondition.OnFirstDeBuffed && !positive &&  _targetPad[input.pos].NeverDeBuffed;
                    bool firstBuffedOrDeBuffed = condition == EffectCondition.OnFirstBuffedOrDeBuffed && _targetPad[input.pos].NeverBuffedOrDeBuffed;

                    match = firstPowerReach || firstBuffed || firstDeBuffed || firstBuffedOrDeBuffed ||
                        (condition == EffectCondition.OnBuffed && positive) ||
                        (condition == EffectCondition.OnDeBuffed && !positive) ||
                        (condition == EffectCondition.OnBuffedOrDeBuffed);;

                    if (firstPowerReach)
                        _targetPad[input.pos].FirstPowerReach = true;

                    if (firstBuffed)
                        _targetPad[input.pos].NeverBuffed = false;

                    if (firstDeBuffed)
                        _targetPad[input.pos].NeverDeBuffed = false;

                    if (firstBuffedOrDeBuffed)
                        _targetPad[input.pos].NeverBuffedOrDeBuffed = false;
                } 
                else
                {
                    match =
                        (condition == EffectCondition.BuffedFriendNum && positive && friend) ||
                        (condition == EffectCondition.BuffedEnemyNum && positive && !friend) ||
                        (condition == EffectCondition.DeBuffedFriendNum && !positive && friend) ||
                        (condition == EffectCondition.DeBuffedEnemyNum && !positive && !friend) ||
                        (condition == EffectCondition.BuffedFriendOrEnemyNum && positive) ||
                        (condition == EffectCondition.DeBuffedFriendOrEnemyNum && !positive);
                }

                if (match)
                {
                    Log.TestLine($"Catch OnChessBuffed: {condition} pos: {input.pos} src: {buffedOne.Position}");
                    EffectOperationHelper.TriggerEffect(_targetPad, input.pos, true, _chessPadManager);
                }
            };
            _buffedHandlers[input.pos] = handler;
            _eventSystem.OnChessBuffed += handler;
        }
        #endregion

        #region Dead Event
        private void UnsubscribeDead(Int2D pos)
        {
            if (_deadHandlers.TryGetValue(pos, out var h))
            {
                _eventSystem.OnChessDead -= h;
                _deadHandlers.Remove(pos);
            }
        }

        public void SubscribeDeadEvent(EffectCondition condition, Input input)
        {
            UnsubscribeDead(input.pos);
            EventHandler<EventSystem.ChessDeadEventArgs> handler = (sender, deadOne) =>
            {
                bool match =
                    condition == EffectCondition.OnFriendOrEnemyDead
                    || (condition == EffectCondition.OnFriendDead &&
                        (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.PLAYER ||
                         deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.RIVAL))
                    || (condition == EffectCondition.OnEnemyDead &&
                        (deadOne.Owner == PlayerType.PLAYER && input.playerType == PlayerType.RIVAL ||
                         deadOne.Owner == PlayerType.RIVAL && input.playerType == PlayerType.PLAYER))
                    || (condition == EffectCondition.OnSelfDead && deadOne.Pos == input.pos);

                if (match)
                {
                    Log.TestLine($"Catch OnChessDead: {condition} Name: {input.chess.Name} pos: {input.pos} src: {deadOne.Chess.Name}");
                    EffectOperationHelper.TriggerEffect(_targetPad, input.pos, true, _chessPadManager);
                }
            };
            _deadHandlers[input.pos] = handler;
            _eventSystem.OnChessDead += handler;
        }
        #endregion
    }
}