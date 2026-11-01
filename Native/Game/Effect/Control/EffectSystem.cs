namespace FFF7RCore.Effect
{
    using System;
    using System.Collections.Generic;
    public class EffectSystem
    {
        private readonly List<PendingEffect> pending_ = new();
        public IReadOnlyList<PendingEffect> PendingEffects => pending_;
        private int nextSeq_;

        public EffectSystem()
        {
            // TODO: Implement
        }

        public void CopyFrom(EffectSystem other)
        {
            pending_.Clear();
            pending_.AddRange(other.pending_);
            nextSeq_ = other.nextSeq_;
        }

        public void WaitFor(EffectCondition cond, Input input, EventKind kind) {
            pending_.RemoveAll(p => p.SourcePos == input.pos && p.Watching == kind);
            pending_.Add(new PendingEffect(input.pos, input.playerType, cond, kind, nextSeq_++, input.chess?.CardCode));
        }

        public void ForgetAll(Int2D pos) => pending_.RemoveAll(p => p.SourcePos == pos);

        public void Dispatch(ChessPad pad, EventKind kind, EventArgs args) {
            List<PendingEffect> view = new List<PendingEffect>(pending_);
            for (int i = 0; i < view.Count; i++) {
                PendingEffect p = view[i];
                if (p.Watching != kind) continue;
                if (!MatchCondition(p, pad, args)) continue;

                Log.TestLine($"Catch {kind}: {p.Condition} pos: {p.SourcePos}");
                EffectOperationHelper.TriggerEffect(pad, p.SourcePos);
            }
        }

        private static bool MatchCondition(PendingEffect p, ChessPad pad, EventArgs args) {
            switch (args) {
                case EventSystem.ChessPlacedEventArgs e:
                    if (p.SourcePos == e.Position) return false;                 // 自己触发自己不算
                    return p.Condition switch {
                        EffectCondition.OnFriendOrEnemyPlayed => true,
                        EffectCondition.OnFriendPlayed => e.Owner == p.SourcePlayer,
                        EffectCondition.OnEnemyPlayed  => e.Owner != p.SourcePlayer,
                        _ => false,
                    };

                case EventSystem.ChessDeadEventArgs e:
                    return p.Condition switch {
                        EffectCondition.OnFriendOrEnemyDead => true,
                        EffectCondition.OnFriendDead => e.Owner == p.SourcePlayer,
                        EffectCondition.OnEnemyDead  => e.Owner != p.SourcePlayer,
                        _ => false,
                    };

                case EventSystem.ChessBuffedEventArgs e:
                    bool positive = e.Value > 0, friend = e.Owner == p.SourcePlayer;
                    if (p.Condition == EffectCondition.OnPowerFirstReach) {
                        PadGrid self = pad[p.SourcePos];
                        bool reach = e.Position == p.SourcePos && !self.Empty && self.Chess.CardEffect != null
                                && self.Level >= self.Chess.CardEffect.PowerThreshold
                                && !self.FirstPowerReach;
                        if (reach) self.FirstPowerReach = true;                  // 唯一的副作用，保留原样
                        return reach;
                    }
                    if (p.SourcePos == e.Position) return false;                 // 自己给自己 buff 不算
                    return p.Condition switch {
                        EffectCondition.BuffedFriendNum          =>  positive &&  friend,
                        EffectCondition.BuffedEnemyNum           =>  positive && !friend,
                        EffectCondition.DeBuffedFriendNum        => !positive &&  friend,
                        EffectCondition.DeBuffedEnemyNum         => !positive && !friend,
                        EffectCondition.BuffedFriendOrEnemyNum   =>  positive,
                        EffectCondition.DeBuffedFriendOrEnemyNum => !positive,
                        _ => false,
                    };
            }
            return false;
        }
    }
}