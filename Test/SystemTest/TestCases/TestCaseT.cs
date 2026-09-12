namespace FFF7RCore.Test {
    public class TestCaseT : TestCase
    {
        public override void ConfigureOptions()
        {
            IgnoreTurnLimit = true;
            Description = "This is a description.";
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
            AddStep(PlayerType.PLAYER, "Card107", new Int2D(1, 0),
                new List<List<int>>{
                new List<int> { F2, O, O, O, E1 },
                new List<int> { FF, F1, O, O, E1 },
                new List<int> { F2, O, O, O, E1 },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 1, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 0 },
                }
            );
            // Step 1
            AddStep(PlayerType.RIVAL, "Card136", new Int2D(2, 4),
                new List<List<int>>{
                new List<int> { F2, O, O, O, E2 },
                new List<int> { FF, F1, O, O, E2 },
                new List<int> { F2, O, O, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 1, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 2
            AddStep(PlayerType.PLAYER, "Card043", new Int2D(1, 1),
                new List<List<int>>{
                new List<int> { F2, F1, O, O, E2 },
                new List<int> { FF, FF, F1, O, E2 },
                new List<int> { F2, O, O, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 1, 2, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 3
            AddStep(PlayerType.RIVAL, "Card031", new Int2D(0, 4),
                new List<List<int>>{
                new List<int> { F2, F1, O, E1, EE },
                new List<int> { FF, FF, F1, O, E3 },
                new List<int> { F2, O, O, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 1 },
                new List<int> { 1, 2, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 4
            AddStep(PlayerType.PLAYER, "Card094", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, F1, F1, E1, EE },
                new List<int> { FF, FF, FF, F1, E3 },
                new List<int> { F2, O, F1, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 1 },
                new List<int> { 1, 2, 2, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 5
            AddStep(PlayerType.RIVAL, "Card010", new Int2D(0, 3),
                new List<List<int>>{
                new List<int> { F2, F1, E1, EE, EE },
                new List<int> { FF, FF, FF, E1, E3 },
                new List<int> { F2, O, F1, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 1, 1 },
                new List<int> { 1, 2, 2, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 6
            AddStep(PlayerType.PLAYER, "Card059", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, F1, F1, EE, EE },
                new List<int> { FF, FF, FF, F1, E3 },
                new List<int> { F2, O, F1, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 1, 1 },
                new List<int> { 1, 2, 1, 0, 0 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 7
            AddStep(PlayerType.RIVAL, "Card047", new Int2D(1, 4),
                new List<List<int>>{
                new List<int> { F2, F1, F1, EE, EE },
                new List<int> { FF, FF, FF, F1, EE },
                new List<int> { F2, O, F1, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 1, 1 },
                new List<int> { 1, 2, 1, 0, 4 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 8
            AddStep(PlayerType.PLAYER, "Card059", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, F1, F2, EE, EE },
                new List<int> { FF, FF, FF, F2, EE },
                new List<int> { F2, O, F1, E1, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 1, 1 },
                new List<int> { 1, 2, 1, 0, 5 },
                new List<int> { 0, 0, 0, 0, 2 },
                }
            );
            // Step 9
            AddStep(PlayerType.RIVAL, "Card013", new Int2D(2, 3),
                new List<List<int>>{
                new List<int> { F2, F1, F2, EE, EE },
                new List<int> { FF, FF, FF, E2, EE },
                new List<int> { F2, O, E1, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 0, 1, 1 },
                new List<int> { 1, 2, 1, 0, 5 },
                new List<int> { 0, 0, 0, 1, 2 },
                }
            );
            // Step 10
            AddStep(PlayerType.PLAYER, "Card071", new Int2D(0, 2),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, E2, EE },
                new List<int> { F2, O, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 1, 2, 1, 0, 5 },
                new List<int> { 0, 0, 0, 1, 2 },
                }
            );
            // Step 11
            AddStep(PlayerType.RIVAL, "Card052", new Int2D(2, 3),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, E3, EE },
                new List<int> { F2, O, E2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 3, 1, 0, 6 },
                new List<int> { 0, 0, 0, 3, 4 },
                }
            );
            // Step 12
            AddStep(PlayerType.PLAYER, "Card086", new Int2D(2, 0),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, E3, EE },
                new List<int> { FF, F1, E2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 3, 1, 0, 6 },
                new List<int> { 3, 0, 0, 3, 4 },
                }
            );
            // Step 13
            AddStep(PlayerType.RIVAL, "Card031", new Int2D(1, 3),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, F1, E2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 3, 1, 1, 6 },
                new List<int> { 3, 0, 0, 5, 4 },
                }
            );
            // Step 14
            AddStep(PlayerType.PLAYER, "Card051", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, F1, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 3, 2, 1, 7 },
                new List<int> { 3, 0, 0, 5, 4 },
                }
            );
            // Step 15
            AddStep(PlayerType.RIVAL, "Card052", new Int2D(2, 3),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, F1, E2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 4, 2, 1, 8 },
                new List<int> { 3, 0, 0, 5, 4 },
                }
            );
            // Step 16
            AddStep(PlayerType.PLAYER, "Card051", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, F1, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 4, 2, 1, 9 },
                new List<int> { 3, 0, 0, 5, 4 },
                }
            );
            // Step 17
            AddStep(PlayerType.RIVAL, "Card059", new Int2D(0, 3),
                new List<List<int>>{
                new List<int> { F2, F1, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, F1, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 5, 2, 1, 10 },
                new List<int> { 3, 0, 0, 5, 4 },
                }
            );
            // Step 18
            AddStep(PlayerType.PLAYER, "Card071", new Int2D(2, 1),
                new List<List<int>>{
                new List<int> { F2, F3, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, FF, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 5, 2, 1, 10 },
                new List<int> { 3, 1, 0, 5, 4 },
                }
            );
            // Step 19
            AddStep(PlayerType.RIVAL, "Card059", new Int2D(0, 4),
                new List<List<int>>{
                new List<int> { F2, F3, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, FF, F2, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 0, 0, 1, 1, 1 },
                new List<int> { 2, 6, 2, 1, 9 },
                new List<int> { 3, 1, 0, 5, 4 },
                }
            );
            // Step 20
            AddStep(PlayerType.PLAYER, "Card134", new Int2D(0, 1),
                new List<List<int>>{
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 1, 2, 5, 1, 1 },
                new List<int> { 4, 6, 2, 1, 9 },
                new List<int> { 7, 1, 1, 5, 8 },
                }
            );
            // Step 21
            AddStep(PlayerType.PLAYER, "Card091", new Int2D(0, 2),
                new List<List<int>>{
                new List<int> { FF, F2, FF, F1, EE },
                new List<int> { FF, FF, F2, E1, EE },
                new List<int> { FF, FF, FF, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 1, 0, 5, 0, 1 },
                new List<int> { 7, 5, 0, 0, 14 },
                new List<int> { 7, 3, 1, 3, 9 },
                }
            );
            // Step 22
            AddStep(PlayerType.RIVAL, "Card013", new Int2D(1, 3),
                new List<List<int>>{
                new List<int> { FF, F2, FF, E1, EE },
                new List<int> { FF, FF, E2, EE, EE },
                new List<int> { FF, FF, FF, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 1, 0, 5, 0, 1 },
                new List<int> { 7, 5, 0, 1, 14 },
                new List<int> { 7, 3, 1, 3, 9 },
                }
            );
            // Step 23
            AddStep(PlayerType.PLAYER, "Card094_1", new Int2D(0, 1),
                new List<List<int>>{
                new List<int> { FF, FF, FF, E1, EE },
                new List<int> { FF, FF, E2, E1, EE },
                new List<int> { FF, FF, F1, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 1, 4, 3, 0, 1 },
                new List<int> { 7, 6, 0, 0, 16 },
                new List<int> { 4, 2, 0, 3, 7 },
                }
            );
            // Step 24
            AddStep(PlayerType.RIVAL, "Card010", new Int2D(1, 2),
                new List<List<int>>{
                new List<int> { FF, FF, FF, E1, EE },
                new List<int> { FF, FF, EE, E1, EE },
                new List<int> { FF, FF, E1, EE, EE },
                },
                new List<List<int>>{
                new List<int> { 1, 4, 3, 0, 1 },
                new List<int> { 7, 6, 1, 0, 16 },
                new List<int> { 4, 2, 0, 3, 7 },
                }
            );
        }
    }
}