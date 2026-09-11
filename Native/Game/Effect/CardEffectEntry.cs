namespace Effect
{
    public class CardEffectEntry
    {
        public EffectCondition Condition;      // 触发条件
    public EffectTarget Target;                // 作用对象范围
        public EffectOperation Operation;      // 执行操作
        public EffectValueType ValueType;      // 数值来源类型

        // 参数
        public int Value;                       // Const模式的固定值
        public int PowerThreshold;              // OnPowerFirstReach 的阈值
        public List<string> CardCodeList;       // 增加的卡牌列表

        public List<List<int>> Scope;
    }
}