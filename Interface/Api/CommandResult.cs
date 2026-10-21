namespace FFF7R
{
    /// <summary>
    /// 指令执行结果。同步返回，不含异常——
    /// 非法输入是正常业务流，不该用异常表达。
    /// 成功时 <see cref="EventsRaised"/> 表示本次调用派发了多少个事件，
    /// 表现层可据此判断是否需要推进动画队列。
    /// </summary>
    public readonly struct CommandResult
    {
        public bool Ok { get; }
        public RejectReason Reason { get; }
        public int EventsRaised { get; }

        private CommandResult(bool ok, RejectReason reason, int eventsRaised)
        {
            Ok = ok;
            Reason = reason;
            EventsRaised = eventsRaised;
        }

        public static CommandResult Success(int eventsRaised = 0)
            => new CommandResult(true, RejectReason.None, eventsRaised);

        public static CommandResult Fail(RejectReason reason)
            => new CommandResult(false, reason, 0);

        public override string ToString()
            => Ok ? "Ok(" + EventsRaised + " events)" : "Rejected: " + Reason;
    }
}
