namespace FFF7RCore.Test {
    public class TestCase2 : TestCase
    {
        public override void ConfigureOptions()
        {
            IgnoreTurnLimit = true;
            Description = "Test Case 2";
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
            AddStep(PlayerType.PLAYER, "Card001", new Int2D(1, 0),
                new List<List<int>>{
                new List<int> { F2,  O,   O ,  O ,  E1 },
                new List<int> { FF,  F1,  O ,  O ,  E1 },
                new List<int> { F2,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  0 },
                new List<int> { 1,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 1
            AddStep(PlayerType.RIVAL, "Card012", new Int2D(0, 4),
                new List<List<int>>{
                new List<int> { F2,  O,   O ,  E1,  EE },
                new List<int> { FF,  F1,  O ,  O ,  E2 },
                new List<int> { F2,  O ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  0,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 2
            AddStep(PlayerType.PLAYER, "Card007", new Int2D(1, 1),
                new List<List<int>>{
                new List<int> { F2,  O,   O ,  E1,  EE },
                new List<int> { FF, FF,  F1 ,  O ,  E2 },
                new List<int> { F2, F1 ,  O ,  O ,  E1 }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  0 }
                }
            );
            // Step 3
            AddStep(PlayerType.RIVAL, "Card013", new Int2D(2, 4),
                new List<List<int>>{
                new List<int> { F2,  O,   O ,  E1,  EE },
                new List<int> { FF, FF,  F1 ,  O ,  E3 },
                new List<int> { F2, F1 ,  O ,  E1,  EE }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  0 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  1 }
                }
            );
            // Step 4
            AddStep(PlayerType.PLAYER, "Card107", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, O ,  F1 ,  E1, EE },
                new List<int> { FF, FF,  FF ,  F1, E3 },
                new List<int> { F2, F1,  F1 ,  E1, EE }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  1 ,  0 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  1 }
                }
            );
            // Step 5
            AddStep(PlayerType.RIVAL, "Card098", new Int2D(1, 4),
                new List<List<int>>{
                new List<int> { F2, O ,  F1 ,  E1, EE },
                new List<int> { FF, FF,  FF ,  E2, EE },
                new List<int> { F2, F1,  F1 ,  E1, EE }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  1 ,  0 ,  7 },
                new List<int> { 0,  0,  0 ,  0 ,  1 }
                }
            );
            // Step 6
            AddStep(PlayerType.PLAYER, "Card008", new Int2D(2, 1),
                new List<List<int>>{
                new List<int> { F2, O ,  F1 ,  E1, EE },
                new List<int> { FF, FF,  FF ,  E2, EE },
                new List<int> { F2, FF,  F2 ,  E1, EE }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  1 ,  0 ,  7 },
                new List<int> { 0,  2,  0 ,  0 ,  1 }
                }
            );
            // Step 7
            AddStep(PlayerType.RIVAL, "Card002", new Int2D(1, 3),
                new List<List<int>>{
                new List<int> { F2, O ,  F1 ,  E2, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { F2, FF,  F2 ,  E2, EE }
                },
                new List<List<int>>{
                new List<int> { 0,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  1 ,  3 ,  7 },
                new List<int> { 0,  2,  0 ,  0 ,  1 }
                }
            );
            // Step 8
            AddStep(PlayerType.PLAYER, "Card012", new Int2D(0, 0),
                new List<List<int>>{
                new List<int> { FF, F1,  F1 ,  E2, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { F2, FF,  F2 ,  E2, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  0,  0 ,  0 ,  1 },
                new List<int> { 1,  2,  2 ,  3 ,  7 },
                new List<int> { 0,  5,  0 ,  0 ,  1 }
                }
            );
            // Step 9
            AddStep(PlayerType.RIVAL, "Card001", new Int2D(0, 3),
                new List<List<int>>{
                new List<int> { FF, F1,  E1 ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { F2, FF,  F2 ,  E2, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  0,  0 ,  1 ,  1 },
                new List<int> { 1,  2,  2 ,  3 ,  7 },
                new List<int> { 0,  5,  0 ,  0 ,  1 }
                }
            );
            // Step 10
            AddStep(PlayerType.PLAYER, "Card098", new Int2D(2, 2),
                new List<List<int>>{
                new List<int> { FF, F1,  E1 ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { F2, FF,  FF ,  F2, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  0,  0 ,  1 ,  1 },
                new List<int> { 1,  2,  2 ,  3 ,  7 },
                new List<int> { 0,  5,  5 ,  0 ,  1 }
                }
            );
            // Step 11
            AddStep(PlayerType.PLAYER, "Card012", new Int2D(0, 1),
                new List<List<int>>{
                new List<int> { FF, FF,  F1 ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { F2, FF,  FF ,  F2, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  1,  0 ,  1 ,  1 },
                new List<int> { 1,  2,  3 ,  3 ,  7 },
                new List<int> { 0,  5,  8 ,  0 ,  1 }
                }
            );
            // Step 12
            AddStep(PlayerType.PLAYER, "Card013", new Int2D(2, 0),
                new List<List<int>>{
                new List<int> { FF, FF,  F1 ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { FF, FF,  FF ,  F2, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  1,  0 ,  1 ,  1 },
                new List<int> { 3,  2,  4 ,  3 ,  7 },
                new List<int> { 1,  5,  8 ,  0 ,  1 }
                }
            );
            // Step 12
            AddStep(PlayerType.PLAYER, "Card013", new Int2D(2, 3),
                new List<List<int>>{
                new List<int> { FF, FF,  F1 ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { FF, FF,  FF ,  FF, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  1,  0 ,  1 ,  1 },
                new List<int> { 3,  2,  4 ,  3 ,  7 },
                new List<int> { 1,  5,  8 ,  1 ,  1 }
                }
            );
            // Step 13
            AddStep(PlayerType.PLAYER, "Card007", new Int2D(0, 2),
                new List<List<int>>{
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { FF, FF,  FF ,  EE, EE },
                new List<int> { FF, FF,  FF ,  FF, EE }
                },
                new List<List<int>>{
                new List<int> { 1,  1,  2 ,  1 ,  1 },
                new List<int> { 3,  2,  4 ,  3 ,  7 },
                new List<int> { 1,  5,  8 ,  1 ,  1 }
                }
            );
        }
    }
}