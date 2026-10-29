namespace FFF7RCore.Test.E2E
{
    using System.Collections.Generic;
    using System.Text;
    using FFF7R;
    using FFF7R.Adapter;

    /// <summary>
    /// 端到端测试：**只通过 <see cref="IGameSession"/> 驱动**，不碰 Game / ChessPad / ChessProperty。
    ///
    /// 为什么必须走接口：这是玩家和 Unity 唯一会走的那条路
    /// （GameSession → KernelMapping → SessionEventBridge）。
    /// 直接 new Game 跑通的用例，证明不了这条链路没断。
    ///
    /// 与 Test/SystemTest 的分工：
    /// <list type="bullet">
    /// <item>SystemTest（剧本用例）：固定盘面 + 指定卡号，验证单张卡的结算结果；</item>
    /// <item>本套（E2E）：随机种子 + 真实手牌 + 真实回合，验证整局能收敛、事件流自洽、同种子可复现。</item>
    /// </list>
    ///
    /// 全部断言用 <see cref="E2ECheck"/> 收集，不抛异常。<see cref="RunAll()"/> 返回是否全绿。
    /// 局数是常量，本地想跑快一点直接改 <see cref="InvariantGames"/> / <see cref="SmokeGames"/>。
    /// </summary>
    public static class E2ERunner
    {
        // 200 局 / 回合上限 24 / 单局最多 300 手，这一组是 S0 验证过的基线（卡死 0）。
        public const int InvariantGames = 200;
        public const int SmokeGames = 40;
        public const int ReplayPairs = 10;
        public const int MaxPliesPerGame = 300;
        public const int TurnLimit = 24;

        public static bool RunAll() => RunAll(new E2ECheck());

        public static bool RunAll(E2ECheck report)
        {
            if (report == null) throw new System.ArgumentNullException(nameof(report));

            // 跑批期间不打逐步日志，摘要最后一次性给
            bool previousPrint = Log.PrintToConsole;
            Log.PrintToConsole = false;

            bool ok;
            try
            {
                ok = Run(report);
            }
            finally
            {
                Log.PrintToConsole = previousPrint;
            }

            Print(report);
            return ok;
        }

        private static bool Run(E2ECheck report)
        {
            ICardCatalog catalog = LoadCatalog();
            List<string> pool = BuildPool(catalog);

            if (pool.Count < 10)
            {
                report.Fail("setup", "可用卡池只有 " + pool.Count + " 张（费用 1~3），不足以开局");
                return false;
            }

            RunInvariant(report, pool);
            RunSmoke(report, pool);
            RunReplay(report, pool);
            RunReject(report, catalog);

            return report.Ok;
        }

        //-------------------------------------------------------------------- 不变式

        /// <summary>
        /// 每手都断言：**GAMING 期间当前方一定至少有一手能走**。
        ///
        /// 这是 S0 修的"卡死"回归防线 —— 客户端表现为"轮到你却点不动任何格子"，
        /// 而它不会报错，只会静默停住，靠终态断言永远抓不到。
        /// </summary>
        private static void RunInvariant(E2ECheck report, List<string> pool)
        {
            int assertionsBefore = report.Assertions;
            int games = 0;
            int plies = 0;
            int stuck = 0;
            int notEnded = 0;
            int instantEnd = 0;

            for (int seed = 1; seed <= InvariantGames; seed++)
            {
                string ctx = "invariant/seed=" + seed;

                using (IGameSession session = Open(pool, seed))
                {
                    games++;
                    int pliesThisGame = 0;

                    CommandResult start = session.Start(PlayerSeat.Player);
                    if (!start.Ok)
                    {
                        report.Fail(ctx, "Start 被拒: " + start.Reason);
                        continue;
                    }

                    int step = 0;
                    for (; step < MaxPliesPerGame; step++)
                    {
                        GameStateView state = session.State;
                        if (!state.IsGaming) break;

                        if (state.Current == PlayerSeat.None)
                        {
                            report.Fail(ctx, "GAMING 中 Current 为 None，ply=" + step);
                            break;
                        }

                        int handIndex;
                        Int2 pos;
                        if (!TryFindMove(session, state.Current, out handIndex, out pos))
                        {
                            stuck++;
                            report.Fail(ctx, "当前方(" + state.Current + ")无任何合法落子，ply=" + step
                                + "，手牌 " + session.Hand(state.Current).Count + " 张");
                            break;
                        }

                        CommandResult placed = session.Place(state.Current, handIndex, pos);
                        if (!placed.Ok)
                        {
                            report.Fail(ctx, "CanPlace 说能下、Place 却被拒: " + placed.Reason);
                            break;
                        }
                        if (placed.EventsRaised == 0)
                        {
                            report.Fail(ctx, "落子成功但一个事件都没派发");
                            break;
                        }

                        plies++;
                        pliesThisGame++;
                    }

                    // 0 手是合法结果：起手双方都没有任何合法落子时，内核在 Start 里就直接终局了
                    // （Game.HasNextTurn 的第 3 条终止条件）。这里只统计，不判失败 —— 但要留痕。
                    if (session.State.IsGaming) notEnded++;
                    if (pliesThisGame == 0) instantEnd++;

                    report.That(!session.State.IsGaming, ctx, "超过 " + MaxPliesPerGame + " 手仍未结束");
                }
            }

            // 空转保护：如果这段代码被改坏成"一局都不跑"，下面的整局断言会全部落空
            report.That(games == InvariantGames, "invariant", "实际开局 " + games + " 局，预期 " + InvariantGames);
            report.That(plies > 0, "invariant", "全部对局一手都没走成，这套测试等于没跑");

            report.Note("invariant: games=" + games + " plies=" + plies + " stuck=" + stuck
                + " notEnded=" + notEnded + " instantEnd=" + instantEnd
                + " checks=" + (report.Assertions - assertionsBefore));
        }

        //-------------------------------------------------------------------- 冒烟

        /// <summary>
        /// 事件流的自洽性：Sequence 连续、GameEnded 唯一且收尾、
        /// ChessPlaced 数量与实际落子次数一致、TurnEnded 的 Turns 只增不减且 pass 不推进。
        /// </summary>
        private static void RunSmoke(E2ECheck report, List<string> pool)
        {
            int assertionsBefore = report.Assertions;
            int games = 0;
            int totalEvents = 0;

            for (int seed = 1; seed <= SmokeGames; seed++)
            {
                string ctx = "smoke/seed=" + seed;

                using (IGameSession session = Open(pool, seed))
                {
                    games++;

                    List<GameEvent> events = new List<GameEvent>();
                    session.Raised += events.Add;   // 必须在 Start 之前订阅，启动时的被动 pass 也要收到

                    CommandResult start = session.Start(PlayerSeat.Player);
                    if (!start.Ok)
                    {
                        report.Fail(ctx, "Start 被拒: " + start.Reason);
                        continue;
                    }

                    int played = 0;
                    for (int step = 0; step < MaxPliesPerGame; step++)
                    {
                        GameStateView state = session.State;
                        if (!state.IsGaming) break;

                        int handIndex;
                        Int2 pos;
                        if (!TryFindMove(session, state.Current, out handIndex, out pos)) break;
                        if (!session.Place(state.Current, handIndex, pos).Ok) break;
                        played++;
                    }

                    CheckSequence(report, ctx, events);
                    CheckGameEnded(report, ctx, events, session.State);
                    CheckPlaced(report, ctx, events, played);
                    CheckTurnTurns(report, ctx, events);
                    CheckAfterEnd(report, ctx, session);

                    totalEvents += events.Count;
                }
            }

            report.Note("smoke: games=" + games + " events=" + totalEvents
                + " checks=" + (report.Assertions - assertionsBefore));
        }

        private static void CheckSequence(E2ECheck report, string ctx, List<GameEvent> events)
        {
            // Sequence 的唯一用途是丢包检测：收到的不是 last+1，说明中间漏了事件
            long expected = 0;
            for (int i = 0; i < events.Count; i++)
            {
                expected++;
                if (events[i].Sequence == expected) continue;
                report.Fail(ctx, "Sequence 不连续：第 " + i + " 个事件是 #" + events[i].Sequence + "，应为 #" + expected);
                return;
            }
        }

        private static void CheckGameEnded(E2ECheck report, string ctx, List<GameEvent> events, GameStateView final)
        {
            int count = 0;
            int lastIndex = -1;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].Kind != GameEventKind.GameEnded) continue;
                count++;
                lastIndex = i;
            }

            report.That(count == 1, ctx, "GameEnded 出现 " + count + " 次，应为 1 次");
            report.That(count == 0 || lastIndex == events.Count - 1, ctx, "GameEnded 不是最后一个事件");
            report.That(final.IsOver, ctx, "跑完仍未结束，状态=" + final.Status);

            if (lastIndex < 0) return;

            GameEndedEvent ended = events[lastIndex] as GameEndedEvent;
            if (ended == null) return;

            report.That(ended.Turns == final.Turns, ctx, "GameEnded.Turns=" + ended.Turns + " 与 State.Turns=" + final.Turns + " 不一致");
            report.That(ended.Winner == final.Winner, ctx, "GameEnded.Winner=" + ended.Winner + " 与 State.Winner=" + final.Winner + " 不一致");
        }

        private static void CheckPlaced(E2ECheck report, string ctx, List<GameEvent> events, int played)
        {
            int placed = 0;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].Kind == GameEventKind.ChessPlaced) placed++;
            }
            report.That(placed == played, ctx, "ChessPlaced=" + placed + " 但成功 Place=" + played);
        }

        private static void CheckTurnTurns(E2ECheck report, string ctx, List<GameEvent> events)
        {
            int last = 0;
            for (int i = 0; i < events.Count; i++)
            {
                TurnEndedEvent turn = events[i] as TurnEndedEvent;
                if (turn == null) continue;

                if (turn.EndedPlayer == turn.NextPlayer)
                {
                    report.Fail(ctx, "TurnEnded 的 EndPlayer 与 NextPlayer 相同: " + turn.EndedPlayer);
                }

                if (turn.IsPass)
                {
                    // 被动跳过只换边，不推进 Turns —— 否则"回合上限 7"实际只够走三四手
                    if (turn.Turns != last)
                    {
                        report.Fail(ctx, "pass 推进了 Turns: " + last + " → " + turn.Turns);
                    }
                }
                else if (turn.Turns != last + 1)
                {
                    report.Fail(ctx, "非 pass 的 TurnEnded 没有 +1: " + last + " → " + turn.Turns);
                }

                last = turn.Turns;
            }
        }

        private static void CheckAfterEnd(E2ECheck report, string ctx, IGameSession session)
        {
            // 终局之后任何落子都必须被拒，且原因要能分辨（不是 Unknown）
            CommandResult after = session.Place(PlayerSeat.Player, 0, new Int2(0, 0));
            report.That(!after.Ok && after.Reason == RejectReason.NotGaming, ctx,
                "终局后 Place 应为 NotGaming，实际 " + after.Reason);
        }

        //-------------------------------------------------------------------- 可复现

        /// <summary>
        /// 同种子 + 同一策略跑两次，事件流必须逐条一致。
        /// 这条一旦挂了，说明引入了未受种子控制的随机源或依赖了字典/哈希的遍历顺序 ——
        /// 崩溃复现、回放、录像全部随之失效。
        /// </summary>
        private static void RunReplay(E2ECheck report, List<string> pool)
        {
            for (int pair = 1; pair <= ReplayPairs; pair++)
            {
                string ctx = "replay/pair=" + pair;
                int seed = pair * 7919;

                List<string> first = PlayToEnd(pool, seed);
                List<string> second = PlayToEnd(pool, seed);

                report.That(first.Count > 0, ctx, "一局下来没有任何事件");

                string diff = null;
                if (first.Count != second.Count)
                {
                    diff = "同种子两次事件数不同: " + first.Count + " vs " + second.Count;
                }
                else
                {
                    for (int i = 0; i < first.Count; i++)
                    {
                        if (first[i] == second[i]) continue;
                        diff = "第 " + i + " 个事件不同\n    A = " + first[i] + "\n    B = " + second[i];
                        break;
                    }
                }

                report.That(diff == null, ctx, diff);
            }

            report.Note("replay: pairs=" + ReplayPairs + "（同种子两次全事件比对）");
        }

        private static List<string> PlayToEnd(List<string> pool, int seed)
        {
            List<string> signatures = new List<string>();

            using (IGameSession session = Open(pool, seed))
            {
                session.Raised += e => signatures.Add(Sign(e));
                session.Start(PlayerSeat.Player);

                for (int step = 0; step < MaxPliesPerGame; step++)
                {
                    GameStateView state = session.State;
                    if (!state.IsGaming) break;

                    int handIndex;
                    Int2 pos;
                    if (!TryFindMove(session, state.Current, out handIndex, out pos)) break;
                    if (!session.Place(state.Current, handIndex, pos).Ok) break;
                }
            }

            return signatures;
        }

        private static string Sign(GameEvent gameEvent)
        {
            switch (gameEvent.Kind)
            {
                case GameEventKind.ChessPlaced:
                {
                    ChessPlacedEvent e = (ChessPlacedEvent)gameEvent;
                    return e.Sequence + "|P|" + e.Pos + "|" + e.Owner + "|" + e.CardCode;
                }
                case GameEventKind.ChessDead:
                {
                    ChessDeadEvent e = (ChessDeadEvent)gameEvent;
                    return e.Sequence + "|D|" + e.Pos + "|" + e.Owner + "|" + e.CardCode;
                }
                case GameEventKind.ChessBuffed:
                {
                    ChessBuffedEvent e = (ChessBuffedEvent)gameEvent;
                    return e.Sequence + "|B|" + e.Pos + "|" + e.Value + "|" + e.Source;
                }
                case GameEventKind.BuffsExpired:
                {
                    BuffsExpiredEvent e = (BuffsExpiredEvent)gameEvent;
                    return e.Sequence + "|X|" + e.Source + "|" + Cells(e.Cells);
                }
                case GameEventKind.GridLevelsChanged:
                {
                    GridLevelsChangedEvent e = (GridLevelsChangedEvent)gameEvent;
                    return e.Sequence + "|G|" + e.Pos + "|" + Cells(e.Cells);
                }
                case GameEventKind.TurnEnded:
                {
                    TurnEndedEvent e = (TurnEndedEvent)gameEvent;
                    return e.Sequence + "|T|" + e.EndedPlayer + "|" + e.NextPlayer + "|" + e.Turns + "|" + (e.IsPass ? "pass" : "play");
                }
                default:
                {
                    GameEndedEvent e = (GameEndedEvent)gameEvent;
                    return e.Sequence + "|E|" + e.Winner + "|" + e.Turns;
                }
            }
        }

        private static string Cells(IReadOnlyList<CellDelta> cells)
        {
            if (cells == null || cells.Count == 0) return "-";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(cells[i].Pos).Append(':').Append(cells[i].Before).Append('>').Append(cells[i].After);
            }
            return sb.ToString();
        }

        //-------------------------------------------------------------------- 拒绝原因

        /// <summary>
        /// 契约层的拒绝原因必须分得清 —— 表现层靠它决定提示文案，
        /// 全都落到 Unknown 就等于没有分类。
        /// </summary>
        private static void RunReject(E2ECheck report, ICardCatalog catalog)
        {
            const string ctx = "reject";

            // 只留费用 1 的牌，保证 (0,0)（标准盘一级格）必定可下
            List<string> pool = new List<string>();
            IReadOnlyList<string> codes = catalog.AllCodes;
            for (int i = 0; i < codes.Count; i++)
            {
                CardDefView def = catalog.Get(codes[i]);
                if (def != null && def.Cost == 1) pool.Add(def.CardCode);
            }

            if (pool.Count < 10)
            {
                report.Fail(ctx, "费用 1 的卡只有 " + pool.Count + " 张，无法开局");
                return;
            }

            using (IGameSession session = Open(pool, 20261021))
            {
                CommandResult start = session.Start(PlayerSeat.Player);
                if (!start.Ok)
                {
                    report.Fail(ctx, "Start 被拒: " + start.Reason);
                    return;
                }

                // ① 不是自己的回合（内核先判回合，再判手牌）
                CommandResult rival = session.Place(PlayerSeat.Rival, 0, new Int2(0, 0));
                report.That(!rival.Ok && rival.Reason == RejectReason.NotYourTurn, ctx,
                    "对手抢先落子应 NotYourTurn，实际 " + rival.Reason);

                // ② 手牌下标越界
                CommandResult badIndex = session.Place(PlayerSeat.Player, 9999, new Int2(0, 0));
                report.That(!badIndex.Ok && badIndex.Reason == RejectReason.HandIndexOutOfRange, ctx,
                    "越界下标应 HandIndexOutOfRange，实际 " + badIndex.Reason);

                // ③ 出界
                CommandResult offBoard = session.Place(PlayerSeat.Player, 0, new Int2(-1, 0));
                report.That(!offBoard.Ok && offBoard.Reason == RejectReason.OutOfBoard, ctx,
                    "出界应 OutOfBoard，实际 " + offBoard.Reason);

                // ④ 合法落子；然后往同一格再下一张，必须是 CellOccupied
                int cheap = FindCardWithCost(session, PlayerSeat.Player, 1);
                report.That(cheap >= 0, ctx, "起手没有费用 1 的牌");
                if (cheap < 0) return;

                CommandResult placed = session.Place(PlayerSeat.Player, cheap, new Int2(0, 0));
                report.That(placed.Ok, ctx, "合法落子被拒: " + placed.Reason);
                report.That(!placed.Ok || placed.EventsRaised > 0, ctx, "合法落子没有派发任何事件");
                if (!placed.Ok) return;

                PlayerSeat actor = session.State.Current;
                int any = FindCardWithCost(session, actor, 1);
                if (any >= 0)
                {
                    CommandResult occupied = session.Place(actor, any, new Int2(0, 0));
                    report.That(!occupied.Ok && occupied.Reason == RejectReason.CellOccupied, ctx,
                        "往已占据格下牌应 CellOccupied，实际 " + occupied.Reason);
                }

                // ⑤ 阶段：已经在 GAMING 时再 Start
                CommandResult again = session.Start(PlayerSeat.Player);
                report.That(!again.Ok && again.Reason == RejectReason.WrongPhase, ctx,
                    "对局中再 Start 应 WrongPhase，实际 " + again.Reason);
            }

            report.Note("reject: NotYourTurn / HandIndexOutOfRange / OutOfBoard / CellOccupied / WrongPhase");
        }

        //-------------------------------------------------------------------- 工具

        private static IGameSession Open(List<string> pool, int seed)
        {
            GameSetup setup = new GameSetup(
                pool,
                pool,
                boardWidth: 3,
                boardHeight: 5,
                standardBoard: true,
                firstSeat: PlayerSeat.Player,
                turnLimit: TurnLimit,
                seed: seed);

            return new GameSession(setup);
        }

        /// <summary>
        /// 借一个空会话把卡牌库读出来。<see cref="GameSession"/> 构造时会自己 Load，
        /// 这里不直接 new CardCatalog 是为了让测试完全走契约层。
        /// </summary>
        private static ICardCatalog LoadCatalog()
        {
            using (IGameSession probe = new GameSession(new GameSetup(new string[0], new string[0])))
            {
                return probe.Catalog;
            }
        }

        private static List<string> BuildPool(ICardCatalog catalog)
        {
            List<string> pool = new List<string>();
            IReadOnlyList<string> codes = catalog.AllCodes;

            for (int i = 0; i < codes.Count; i++)
            {
                CardDefView def = catalog.Get(codes[i]);
                if (def == null) continue;
                // 0 费是覆盖型，落在空格上必被拒；3 费以上开局几乎无处可下。两者都不进随机对局
                if (def.Cost < 1 || def.Cost > 3) continue;
                pool.Add(def.CardCode);
            }

            return pool;
        }

        private static int FindCardWithCost(IGameSession session, PlayerSeat seat, int cost)
        {
            if (seat == PlayerSeat.None) return -1;

            HandSnapshot hand = session.Hand(seat);
            for (int i = 0; i < hand.Count; i++)
            {
                CardDefView def = hand[i].Def;
                if (def != null && def.Cost == cost) return hand[i].HandIndex;
            }
            return -1;
        }

        /// <summary>
        /// 用 <see cref="IGameQuery.CanPlace"/> 找第一步能下的棋。
        /// 扫描顺序固定（牌序 → x → y），同种子下两次跑出来的着法必须完全一致，
        /// 这是 <see cref="RunReplay"/> 能成立的前提。
        /// </summary>
        private static bool TryFindMove(IGameSession session, PlayerSeat seat, out int handIndex, out Int2 pos)
        {
            handIndex = -1;
            pos = new Int2(-1, -1);

            if (seat == PlayerSeat.None) return false;

            HandSnapshot hand = session.Hand(seat);
            if (hand.Count == 0) return false;

            BoardSnapshot board = session.Board;
            int width = board.Width;
            int height = board.Height;

            for (int i = 0; i < hand.Count; i++)
            {
                int index = hand[i].HandIndex;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Int2 candidate = new Int2(x, y);
                        RejectReason reason;
                        if (!session.CanPlace(seat, index, candidate, out reason)) continue;

                        handIndex = index;
                        pos = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void Print(E2ECheck report)
        {
            const int maxFailuresToPrint = 40;

            Log.TestLine("\n===== E2E / 冒烟 =====", TextColor.BLUE);

            for (int i = 0; i < report.Notes.Count; i++)
            {
                Log.TestLine("  " + report.Notes[i], TextColor.YELLOW);
            }

            if (report.Ok)
            {
                Log.TestLine("  通过: " + report.Assertions + " 条断言", TextColor.GREEN);
            }
            else
            {
                Log.TestLine("  失败: " + report.Failed + " / " + report.Assertions + " 条断言", TextColor.RED);
                int limit = report.Failures.Count < maxFailuresToPrint ? report.Failures.Count : maxFailuresToPrint;
                for (int i = 0; i < limit; i++)
                {
                    Log.TestLine("    " + report.Failures[i], TextColor.RED);
                }
                if (report.Failures.Count > limit)
                {
                    Log.TestLine("    ...还有 " + (report.Failures.Count - limit) + " 条未列出", TextColor.RED);
                }
            }

            Log.TestLine("======================", TextColor.BLUE);
        }
    }
}
