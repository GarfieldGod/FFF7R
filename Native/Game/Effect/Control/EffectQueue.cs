namespace FFF7RCore.Effect {
    public class EffectQueue
    {
        public bool HasTask => _taskQueue.Count > 0;
        public bool IsRunning { get; private set; }
        public int Count => _taskQueue.Count;

        private Queue<Input> _taskQueue = new Queue<Input>();
        private ChessPadManager _owner;

        public EffectQueue(ChessPadManager owner)
        {
            _owner = owner;
        }

        public void Enqueue(Input task)
        {
            Log.TestLine($"Task Enqueue: {task.pos} {task.playerType} {task.chess.CardCode} {task.chess.CardEffect.Condition} {task.chess.CardEffect.Operation}", TextColor.BLUE);
            _taskQueue.Enqueue(task);
        }

        public void RunNext()
        {
            if (IsRunning || !HasTask) return;
            IsRunning = true;

            var task = _taskQueue.Dequeue();
            Log.TestLine($"Task Dequeue: {task.pos} {task.playerType} {task.chess.CardCode} {task.chess.CardEffect.Condition} {task.chess.CardEffect.Operation}", TextColor.GREEN);
            EffectOperationHelper.TriggerEffect(_owner, task, !(task.chess.CardEffect.Condition == EffectCondition.OnPosition));
            IsRunning = false;
        }

        public void Clear()
        {
            _taskQueue.Clear();
            IsRunning = false;
        }
    }
}