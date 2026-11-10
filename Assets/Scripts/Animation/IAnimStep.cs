public interface IAnimStep
{
    void Begin();            // 捕获起点。必须在播放前调用，不能在 Enqueue 时调
    bool Tick(float dt);     // 返回 true = 播完
    void Finish();           // 直接跳到终态（跳帧 / 超时 / Resync 用）
}

public interface IAnimGate { bool IsBusy { get; } }