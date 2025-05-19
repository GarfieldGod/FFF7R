# FFF7R

## Introduction

Algorithmic programs replicate the card game mechanics [ From Final Fantasy 7 Rebirth ].

## Attention

~~The latest update mostly appears in the UnityProject branch.~~

In order to better decouple the logical code from the Unity-specific code, the update of the Unity project has been temporarily suspended.

**Now, the game and test cases can be executed in the console without relying on Unity.**

## Test Case

### Example

For example, refer to `Native/Console/TestCases/TestCase1.cs` for more information.

### Add new test cases

To add a new test case, you need to create a new class under `Native/Console/Entry/TestCase` and inherit it from `TestCase`, for example, `NewTestCase : TestCase`.

1. Override the `ChessPad InitChessPad()` method to initialize the chessboard.
2. Override the `void InitSteps()` method to add the steps for the test case.

### Init the chessboard

init the test chessboard at the start of the game by override the `ChessPad InitChessPad()` method.

### Add test steps

`AddStep(InputerType inputerType, int index, string cardCode, List<List<int>> expectPad1, List<List<int>> expectPad2)`

You should use the `AddStep` method inside the `void InitSteps()` method to define the steps for your test case.

#### Parameters Explanation:

* `inputerType`: Indicates who performed the input on the chessboard. It can be either `InputerType.PLAYER` or `InputerType.RIVAL`.
* `index`: Specifies which grid on the chessboard you want to input to. The range is based on the size of the chessboard.
* `cardCode`: Specifies which card you want to input. For more information, see `Json/ChessProperties.json`.
* `expectPad1`: Represents the expected grid levels on the chessboard. It should have the same meaning as the corresponding effects in FF7Rb.
* `expectPad2`: Represents the expected card levels on the chessboard. It should have the same meaning as the corresponding effects in FF7Rb.

### Run

Create an instance of your test case class and call the `Run()` method to execute the test steps you have added, in `Native/Console/Entry/Main.cs`.

### Notes

* Ensure that the `cardCode` matches one of the valid codes defined in `Json/ChessProperties.json`.
* Ensure that your input is valid. Note that if the `InputerType` is `RIVAL`, the `GridEffect` and `CardEffect` caused by the input will be reversed.

## Console Game

It is not recommended to run the console game, as it is currently not being maintained.

Try it from `Native/Console/Entry/Main.cs` using the `StartConsoleGame()` method.
