// public class EffectQueue
// {
//     // 任务队列
//     private Queue<EffectTask> _taskQueue = new Queue<EffectTask>();

//     /// <summary>是否正在执行队列，防止重入递归</summary>
//     public bool IsRunning { get; private set; }

//     /// <summary>入队一个效果任务</summary>
//     public void Enqueue(EffectTask task)
//     {
//         _taskQueue.Enqueue(task);
//     }

//     /// <summary>按阶段执行：只执行当前阶段全部任务，执行完停下，不自动跑下一阶段</summary>
//     public void RunPhase(EffectPhase phase)
//     {
//         if (IsRunning) return;
//         IsRunning = true;

//         // 取出当前阶段所有任务，其余保留在队列
//         List<EffectTask> currentPhaseTasks = new List<EffectTask>();
//         int count = _taskQueue.Count;
//         for (int i = 0; i < count; i++)
//         {
//             var t = _taskQueue.Dequeue();
//             if (t.Phase == phase)
//             {
//                 currentPhaseTasks.Add(t);
//             }
//             else
//             {
//                 _taskQueue.Enqueue(t);
//             }
//         }

//         // 批量执行本阶段所有任务
//         foreach (var task in currentPhaseTasks)
//         {
//             task.Execute?.Invoke(task);
//         }

//         IsRunning = false;
//     }

//     /// <summary>直接执行队列里剩下全部（全部阶段按入队顺序）</summary>
//     public void RunAllRemaining()
//     {
//         if (IsRunning) return;
//         IsRunning = true;
//         while (_taskQueue.Count > 0)
//         {
//             var task = _taskQueue.Dequeue();
//             task.Execute?.Invoke(task);
//         }
//         IsRunning = false;
//     }

//     /// <summary>清空队列，中断所有未执行效果</summary>
//     public void Clear()
//     {
//         _taskQueue.Clear();
//         IsRunning = false;
//     }

//     public bool HasTask => _taskQueue.Count > 0;
// }
