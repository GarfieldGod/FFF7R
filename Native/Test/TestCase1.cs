// ChessPad Index:
// 00 01 02 03 04
// 05 06 07 08 09
// 10 11 12 13 14
// TODO
// Add Success or Failed for Step
// Add PadIndex convert for Rival Step
namespace Test
{
    public class TestCase1 : TestCase
    {
        public override ChessPad InitChessPad()
        {
            ChessPad initChessPad = new ChessPad();
            initChessPad.InitStandard();
            return initChessPad;
        }
        public override void InitSteps()
        {
            // Step 0
            AddStep(InputerType.PLAYER,
                0, "CardTest1",
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
            AddStep(InputerType.PLAYER,
                1, "CardTest1",
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
            AddStep(InputerType.RIVAL,
                0, "CardTest1",
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
            AddStep(InputerType.PLAYER,
                2, "CardTest1",
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
            AddStep(InputerType.RIVAL,
                5, "Card009",
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
            AddStep(InputerType.RIVAL,
                10, "CardTest1",
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
            AddStep(InputerType.RIVAL,
                11, "CardTest0",
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
            AddStep(InputerType.RIVAL,
                1, "CardTest0",
                new List<List<int>>{
                new List<int> { FF,  FF,  FF,  EE,  E1 },
                new List<int> { F2,  F1,  F1,  E3,  EE },
                new List<int> { F1,  O ,  E1,  EE,  EE }
                },
                new List<List<int>>{
                new List<int> { 2,  3,  1 ,  2 ,  0 },
                new List<int> { 0,  0,  0 ,  0 ,  3 },
                new List<int> { 0,  0,  0 ,  2 ,  1 }
                }
            );
            // // Step 8
            // AddStep(InputerType.RIVAL,
            //     0, "CardTest2",
            //     new List<List<int>>{
            //     new List<int> { FF,  FF,  FF,  E1,  EE },
            //     new List<int> { F2,  F1,  F1,  E3,  E1 },
            //     new List<int> { F1,  O ,  E1,  EE,  EE }
            //     },
            //     new List<List<int>>{
            //     new List<int> { 2,  3,  1 ,  2 ,  0 },
            //     new List<int> { 0,  0,  0 ,  0 ,  3 },
            //     new List<int> { 0,  0,  0 ,  2 ,  1 }
            //     }
            // );
        }
    }
}