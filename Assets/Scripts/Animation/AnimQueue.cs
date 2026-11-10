using System.Collections.Generic;

public sealed class AnimQueue : IAnimGate
{
    readonly Queue<IAnimStep> _pending = new();
    IAnimStep _current;

    public bool IsBusy => _current != null || _pending.Count != 0;
    public float Speed = 1f;

    public void Enqueue(IAnimStep step)
    {
        if(step != null) _pending.Enqueue(step);
    }

    public void Tick(float dt)
    {
        while (true)
        {
            if (_current == null)
            {
                if (_pending.Count == 0) return;
                _current = _pending.Dequeue();
                _current.Begin();          // ← 只在轮到时 Begin
            }
            if (!_current.Tick(dt * Speed)) return;
            _current.Finish();
            _current = null;
            // 本帧剩余时间不滚入下一步，避免一步动画吃掉下一步的时长
            return;
        }
    }

    public void FinishAll() { 
        while (_current != null || _pending.Count > 0) { 
            if (_current != null) { _current.Finish(); _current = null; } 
            if (_pending.Count > 0) { _current = _pending.Dequeue(); _current.Begin(); }
        }
    }

    public void Clear() { 
        _pending.Clear(); 
        _current?.Finish(); 
        _current = null; 
    }
}
