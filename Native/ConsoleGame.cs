public class ConsoleGame : Game {
    public ConsoleGame(SingleGameConfig singleGameConfig) : base(singleGameConfig) {}
    public void ShowCardInHand() {
        Log.TestLine("Cards:", TextColor.BLUE, true);
        int index = 0;
        foreach (var chess in player_.GetChessInHand()) {
            string name = Utils.FixLength(chess.GetChessProperty().Name, 10);
            Log.TestLine(
                index.ToString() + 
                "\t[" + name + "]" + 
                "\tCost: " + chess.GetChessProperty().Cost.ToString() + 
                "\tLevel: " + chess.GetChessProperty().Level.ToString()
            );
            index++;
        }
    }

    public static void ShowChessGrid(ChessPad chessPad) {
        for(int x = 0;x < chessPad.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad.GetChessGridStatus()[0].Count ; y++) {
                Chess chess = chessPad.GetChessStatus()[x][y];
                int posStatus = chessPad.GetChessGridStatus()[x][y];
                TextColor textColor;
                bool ifHightLight = true;
                if ((posStatus > 10 && posStatus < 14) || posStatus == 15) {
                    textColor = TextColor.RED;
                } else if (posStatus < 10 || posStatus == 14) {
                    textColor = TextColor.GREEN;
                } else {
                    textColor = TextColor.NONE;
                    ifHightLight = false;
                }

                string output = (posStatus % 10).ToString();
                if (chess != null) {
                    output = chess.GetChessProperty().Name;
                }
                Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
            }
            Log.Test("\n");
        }
    }

    public void ShowChessGrid() {
        Log.TestLine("The ChessGrid:", TextColor.BLUE, true);
        int index = 0;
        for(int x = 0;x < chessPad_.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad_.GetChessGridStatus()[0].Count ; y++) {
                Chess chess = chessPad_.GetChessStatus()[x][y];
                int posStatus = chessPad_.GetChessGridStatus()[x][y];
                TextColor textColor;
                bool ifHightLight = true;
                if ((posStatus > 10 && posStatus < 14) || posStatus == 15) {
                    textColor = TextColor.RED;
                } else if (posStatus < 10 || posStatus == 14) {
                    textColor = TextColor.GREEN;
                } else {
                    textColor = TextColor.NONE;
                    ifHightLight = false;
                }

                string output = (posStatus % 10).ToString();
                if (chess != null) {
                    output = chess.GetChessProperty().Name;
                }
                Log.Test("(" + index.ToString("D2") + ")", TextColor.BLACK, false);
                Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
                index++;
            }
            Log.Test("\n");
        }
    }

    public void ShowChess() {
        Log.TestLine("The Chess:", TextColor.BLUE, true);
        for(int x = 0;x < chessPad_.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad_.GetChessGridStatus()[0].Count ; y++) {
                Chess chess = chessPad_.GetChessStatus()[x][y];
                int posStatus = chessPad_.GetChessGridStatus()[x][y];
                TextColor textColor;
                bool ifHightLight = true;
                if ((posStatus > 10 && posStatus < 14) || posStatus == 15) {
                    textColor = TextColor.RED;
                } else if (posStatus < 10 || posStatus == 14) {
                    textColor = TextColor.GREEN;
                } else {
                    textColor = TextColor.NONE;
                    ifHightLight = false;
                }

                string output = "null";
                if (chess != null) {
                    output = chess.GetChessProperty().Name;
                }
                Log.Test(Utils.FixLength(output, 15), textColor, ifHightLight);
            }
            Log.Test("\n");
        }
    }

    Dictionary<int, Int2D> chessGridIndexMap_ = [];

    public override ChessPad InitChessPadStandard() {
        ChessPad chessPad = new ChessPad(
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 }
            },
            new List<List<Chess>>{
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null },
                new List<Chess> { null, null, null, null, null }
            },
            new List<List<int>>{
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 0 },
                new List<int> { 0, 0, 0, 0, 0 }
            },
            new List<List<List<Buff>>>{
                new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} },
                new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} },
                new List<List<Buff>> { new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{}, new List<Buff>{} }
            },
            new List<List<int>>{
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 },
                new List<int> { 1, 10, 10, 10, 11 }
            }
        );
        int index = 0;
        for(int x = 0;x < chessPad.GetChessGridStatus().Count ; x++) {
            for(int y = 0;y < chessPad.GetChessGridStatus()[0].Count ; y++) {
                chessGridIndexMap_.Add(index, new Int2D(x,y));
                index++;
            }
        }
        return chessPad;
    }

    public void ShowInfo(string info, TextColor textColor = TextColor.NONE) {
        // Log.Clear();
        Log.TestLine("Turns: " + GameTurns, TextColor.BLUE, true);
        ShowChessGrid();
        ShowChess();
        ShowCardInHand();
        Log.Test("Info:\n", TextColor.BLUE, true);
        Log.TestLine(info, textColor, true);
    }
    public override void RunGameTurns(int TimeEveryTurn) {
        while(gameStatus_ == GameStatus.GAMING) {
            base.RunGameTurns(TimeEveryTurn);
        }
    }
    public override void AiTurn() {
        ShowInfo(turnInfoText_, TextColor.RED);
        int delayTime = 1;
        Thread.Sleep(1000 * delayTime);
        ShowChessGrid(rival_.GetChessPad());
        Input input = AiRival.GetTheBestInput(rival_.GetChessPad(), rival_.GetChessInHand());
        if (rival_.CanInput() && !input.Empty())
        {
            Log.TestLine("Rival Do Input:" + " X: " + input.pos.x.ToString() + " Y: " + input.pos.y.ToString());
            if (rival_.Select(input.chess) != -1) {
                Log.TestLine("Rival Select Success.", TextColor.RED);
                Console.ReadLine();
                if (rival_.AddInput(input)) {
                    ShowInfo("Rival AddInput Success", TextColor.RED);
                    Console.ReadLine();
                    rival_.CommitInput();
                } else {
                    rival_.RestoreSelect();
                    ShowInfo("Rival AddInput Failed", TextColor.RED);
                }
            } else {
                Log.TestLine("Rival Select Failed.", TextColor.RED);
            }
        }
        ShowInfo("Rival Finished", TextColor.RED);
    }
    public override void RivalTurn() {
        AiTurn();
        base.RivalTurn();
    }
    public override void PlayerTurn()
    {
        PlayerTurnWithInfo("Select Card", TextColor.YELLOW);
        base.PlayerTurn();
    }
    private void PlayerTurnWithInfo(string info, TextColor textColor) {
        ShowInfo(info, textColor);

        string select = Console.ReadLine();
        if (int.TryParse(select, out int index) && index >= 0 && index < player_.GetChessInHand().Count &&
            (player_.Select(index) is { } selectedChess)) {

            ShowInfo(
                "Selected Card:" + selectedChess.GetChessProperty().Name +
                "\nSelect Grid To ADD:",
                TextColor.YELLOW
            );
            string input = Console.ReadLine();
            if (int.TryParse(input, out int grid) && grid >= 0 && grid < 15 && 
                player_.AddInput(new Input(chessGridIndexMap_[grid], selectedChess))) {

                ShowInfo(
                    "Selected Card: " + selectedChess.GetChessProperty().Name +
                    "\nSelected Grid: " + input +
                    "\nEnter 1 To Commit, 0 To Cancel:",
                    TextColor.YELLOW
                );
                string commit = Console.ReadLine();
                if (int.TryParse(commit, out int result) && result == 1) {
                    player_.CommitInput();
                } else {
                    player_.RestoreInput();
                    PlayerTurnWithInfo("Cancel Commit, Select Card:", TextColor.YELLOW);
                    return;
                }
            } else {
                player_.RestoreSelect();
                PlayerTurnWithInfo("Invaild Input! ReSelect Card!", TextColor.RED);
                return;
            }
        } else {
            PlayerTurnWithInfo("Invaild Input! ReSelect Card!", TextColor.RED);
            return;
        }
    }
}