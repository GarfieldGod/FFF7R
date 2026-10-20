using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    private FFF7RCore.Game InnerGame_;
    private ChessBoard chessBoard;

    void Start()
    {
        InitGame();
        StartGame();
    }

    void InitGame()
    {
        var initPad = InitChessPad();
        var playerChessPool = new List<string>{};
        var rivalChessPool = new List<string>{};
        FFF7RCore.GameConfig gameConfig = new FFF7RCore.GameConfig(playerChessPool, rivalChessPool, initPad);
        InnerGame_ = new FFF7RCore.Game(gameConfig);
        InnerGame_.Init();
    }

    void StartGame()
    {
        if (InnerGame_.Status == FFF7RCore.GameStatus.Ready)
        {
            InnerGame_.Start(FFF7RCore.PlayerType.PLAYER);
        }
    }

    public FFF7RCore.ChessPad InitChessPad()
    {
        FFF7RCore.ChessPad initChessPad = new FFF7RCore.ChessPad(3, 5);
        initChessPad.InitStandard();
        return initChessPad;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
