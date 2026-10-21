namespace FFF7RCore.Test {
    using System.Collections.Generic;
    public class TestCase1 : TestCase
    {
        public override void ConfigureOptions()
        {
            // IgnoreTurnLimit = false;          是否忽略该回合是谁的回合的限制
            // StopTestCaseWhenFailed = true;    是否在测试用例失败时停止测试
            // NoLog = false;                    是否不打印日志
            // Description = "No description.";  用例描述
            IgnoreTurnLimit = true;
            Description = "Test Case 1";
        }

        public override ChessPad InitChessPad()
        {
            ChessPad initChessPad = new ChessPad(3, 5);
            initChessPad.InitStandard();
            return initChessPad;
        }

        public override void InitSteps()
        {
            // Step 0
            AddStep(PlayerType.PLAYER, "PosFriend4Up1", new Int2D(0, 0),
                new List<List<int>>{
                new List<int> { FF,  F1,  O ,  O ,  E1 },
                new List<int> { F2,  O ,  O ,  O ,  E1 },
                new List<int> { F1,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 1,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 1
            AddStep(PlayerType.PLAYER, "PosFriend4Up1", new Int2D(0, 1),
                new List<List<int>>{
                new List<int> { FF,  FF,  F1,  O ,  E1 },
                new List<int> { F2,  F1,  O ,  O ,  E1 },
                new List<int> { F1,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  2,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 2
            AddStep(PlayerType.RIVAL, "PosFriend4Up1", new Int2D(0, 4),
                new List<List<int>>{
                new List<int> { FF,  FF,  F1,  E1,  EE },
                new List<int> { F2,  F1,  O ,  O ,  E2 },
                new List<int> { F1,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  2,  0 ,  0 ,  1 },
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 3
            AddStep(PlayerType.PLAYER, "PosFriend4Up1", new Int2D(0, 2),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  F1,  EE },
                new List<int> { F2,  F1,  F1,  O ,  E2 },
                new List<int> { F1,  O ,   O,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  1 },
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 4
            AddStep(PlayerType.RIVAL, "CardTest4", new Int2D(1, 4),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E1,  EE },
                new List<int> { F2,  F1,  F1,  E1,  EE },
                new List<int> { F1,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  1 },
                new List<int> { 0,  0,  0 ,  0 ,  2 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 5
            AddStep(PlayerType.RIVAL, "PosFriend4Up1", new Int2D(2, 4),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E1,  EE },
                new List<int> { F2,  F1,  F1,  E1,  EE },
                new List<int> { F1,  O ,  O ,  E1,  EE }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  1 },
                new List<int> { 0,  0,  0 ,  0 ,  3 },
                new List<int> { 0,  0,  0 ,  0 ,  2 }
                }
            );
            // Step 6
            AddStep(PlayerType.RIVAL, "PosAll4Down1", new Int2D(2, 3),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E1,  EE },
                new List<int> { F2,  F1,  F1,  E2,  EE },
                new List<int> { F1,  O ,  E1,  EE,  EE }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  1 },
                new List<int> { 0,  0,  0 ,  0 ,  3 },
                new List<int> { 0,  0,  0 ,  2 ,  1 }
                }
            );
            // Step 7
            AddStep(PlayerType.RIVAL, "PosAll4Down1", new Int2D(0, 3),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  EE,  E2 },
                new List<int> { F2,  F1,  F1,  E3,  EE },
                new List<int> { F1,  O ,  E1,  EE,  EE }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  1 ,  1 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  2 },
                new List<int> { 0,  0,  0 ,  2 ,  1 }
                }
            );
            // Step 8
            AddStep(PlayerType.RIVAL, "FirstDeBuffedAll4Down2", new Int2D(0, 4),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E2,  EE },
                new List<int> { F2,  F1,  F1,  E3,  E2 },
                new List<int> { F1,  O ,  E1,  EE,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  2 },
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  1 ,  0 }
                }
            );
            // Step 9
            AddStep(PlayerType.PLAYER, "FirstDeBuffedAll4Down2", new Int2D(1, 1),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E2,  EE },
                new List<int> { F3,  FF,  F2,  E3,  E2 },
                new List<int> { F1,  F1 , E1,  EE,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  2 ,  0 ,  2 },
                new List<int> { 0,  3,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  1 ,  0 }
                }
            );
            // Step 10
            AddStep(PlayerType.PLAYER, "PosAll4Down1", new Int2D(2, 1),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E2,  EE },
                new List<int> { F3,  FF,  F2,  E3,  E2 },
                new List<int> { F2,  F1 , F1,  EE,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  1,  2 ,  0 ,  2 },
                new List<int> { 0,  3,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  1 ,  0 }
                }
            );
            // Step 11
            AddStep(PlayerType.PLAYER, "PosAll4Down1", new Int2D(2, 1),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E2,  EE },
                new List<int> { F3,  FF,  F2,  E3,  E2 },
                new List<int> { F3,  FF,  F2,  EE,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  1,  2 ,  0 ,  2 },
                new List<int> { 0,  2,  0 ,  0 ,  0 },
                new List<int> { 0,  1,  0 ,  1 ,  0 }
                }
            );
            // Step 12
            AddStep(PlayerType.RIVAL, "CardTest5", new Int2D(1, 3),
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  E3,  EE },
                new List<int> { F3,  FF,  E2,  EE,  E3 },
                new List<int> { F3,  FF,  F2,  EE,  E1 }
                },
                new List<List<int>>{
                new List<int> { 2,  1,  2 ,  0 ,  2 },
                new List<int> { 0,  2,  0 ,  1 ,  0 },
                new List<int> { 0,  1,  0 ,  1 ,  0 }
                }
            );
            // Step 13
            AddStep(PlayerType.RIVAL, "PosAll4Down1", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { FF,  F1,  E1,  E3,  E1 },
                new List<int> { F3,  FF,  E1,  E2,  E3 },
                new List<int> { F3,  FF,  E2,  E1,  E1 }
                },
                new List<List<int>>{
                new List<int> { 1,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  1,  0 ,  0 ,  0 },
                new List<int> { 0,  1,  0 ,  0 ,  0 }
                }
            );
        }
    }
}