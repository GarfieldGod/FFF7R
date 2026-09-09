// using System.ComponentModel;
// using Test;

// public class EffectsMaker {
//     public EffectsMaker(List<List<int>> PosStatusMap, PlayerType playerType = PlayerType.PLAYER) {
//         posStatus_ = PosStatusMap;
//         inputerType_ = playerType;
//     }
//     private Dictionary<Int2D, Tuple<ChessProperty, int>> effectTasks_ = new Dictionary<Int2D, Tuple<ChessProperty, int>>();
//     private PlayerType inputerType_;
//     private List<List<int>> posStatus_;
//     private List<List<int>> effectStatusBase_ = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);
//     private List<List<int>> effectStatusEnhanced_ = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);

//     public bool AddEffectsTask(Int2D effectPos, ChessProperty property) {
//         if (effectTasks_.ContainsKey(effectPos)) {
//             return false;
//         }
//         if (property.CardEffectConfig.condition < EffectCondition.ON_POSITION) {
//             CardEffect.ParseCardEffect(effectPos, property, effectStatusEnhanced_, UsePosStatus());
//         } else if (property.CardEffectConfig.condition >= EffectCondition.CoverInput) {
//             //special
//         } else {
//             //many times
//             int initTimes = 0;
//             if (property.CardEffectConfig.condition == EffectCondition.ON_POSITION) {
//                 initTimes = 1;
//             }
//             effectTasks_.Add(effectPos, new Tuple<ChessProperty, int>(property, initTimes));
//         }
//         return true;
//     }
//     public bool RemoveEffectsTask(Int2D effectPos) {
//         if (effectTasks_.ContainsKey(effectPos)) {
//             effectTasks_.Remove(effectPos);
//             return true;
//         }
//         return false;
//     }
//     public int GetEffectStatus(Int2D effectPos) {
//         return GetEffectStatusBase(effectPos) + GetEffectStatusEnhanced(effectPos);
//     }
//     public int GetEffectStatusBase(Int2D effectPos) {
//         return effectStatusBase_[effectPos.x][effectPos.y];
//     }
//     public int GetEffectStatusEnhanced(Int2D effectPos) {
//         return effectStatusEnhanced_[effectPos.x][effectPos.y];
//     }
//     public List<List<int>> ComposeCardEffectsMap() {
//         List<List<int>> effectEnhancedFinal = Utils.Compose2DList(effectStatusEnhanced_, CalculateEnhancedEffectsList());
//         return Utils.Compose2DList(effectEnhancedFinal, effectStatusBase_);
//     }
//     public List<List<int>> CalculateEnhancedEffectsList() {
//         List<List<int>> result = Utils.DeepCopy2DList(Utils.EmptyStandard2DList);
//         foreach(var effectTask in effectTasks_) {
//             for(int i = 0; i <= effectTask.Value.Item2;i++) {
//                 CardEffect.ParseCardEffect(effectTask.Key, effectTask.Value.Item1, result, UsePosStatus());
//             }
//         }
//         return result;
//     }
//     public void TriggerEffectByCondition(EffectCondition condition){
//         List<Int2D> pos = new List<Int2D>();
//         foreach(var effectTask in effectTasks_) {
//             if (effectTask.Value.Item1.CardEffectConfig.condition == condition) {
//                 pos.Add(effectTask.Key);
//             }
//         }
//         foreach(var vaildPos in pos) {
//             effectTasks_[vaildPos] = new Tuple<ChessProperty, int>(effectTasks_[vaildPos].Item1, effectTasks_[vaildPos].Item2 + 1);
//         }
//     }
//     private List<List<int>> UsePosStatus(){
//         List<List<int>> result = Utils.DeepCopy2DList(posStatus_);
//         if (inputerType_ == PlayerType.RIVAL) {
//             Rival.GetChessPosStatusInRivalView(result);
//         }
//         return result;
//     }
//     private List<List<int>> UseEffectStatusBase(){
//         List<List<int>> result = Utils.DeepCopy2DList(effectStatusBase_);
//         if (inputerType_ == PlayerType.RIVAL) {
//             Rival.GetChessLevelStatusInRivalView(result);
//         }
//         return result;
//     }
//     private List<List<int>> UseEffectStatusEnhanced(){
//         List<List<int>> result = Utils.DeepCopy2DList(effectStatusEnhanced_);
//         if (inputerType_ == PlayerType.RIVAL) {
//             Rival.GetChessLevelStatusInRivalView(result);
//         }
//         return result;
//     }
// }