class TestCase1 {
    private ChessPad initChessPad = new ChessPad();
    bool Run() {
        initChessPad.InitStandard();
        TestSuite testSuite = new TestSuite(initChessPad);
        testSuite.AddStep(new Step(
            InputerType.PLAYER,
            new Input(
                new Int2D(),
            ),
            new ChessPad(

            )
        ));
        testSuite.AddStep(new Step(
            InputerType.PLAYER,
            new Input(

            ),
            new ChessPad(

            )
        ));
        return testSuite.Run();
    }
}