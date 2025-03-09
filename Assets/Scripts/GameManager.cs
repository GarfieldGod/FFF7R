using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    ChessDispenser chessDispenser;
    void Start()
    {
        chessDispenser = GetComponent<ChessDispenser>();
        StartAGame();
    }
    void StartAGame() {
        ChessDispenser.chessPool = new List<string>{
            "Card001", "Card001", "Card001", "Card001", "Card001",
            "Card002", "Card002", "Card002", "Card002", "Card002",
            "Card003", "Card003", "Card003", "Card003", "Card003",
        };
        Debug.Log("Instantiate chessPool success.");
        for( int i = 0 ; i < 5 ; i++ ) {
            ChessSelector.PushBackChess(chessDispenser.InstantiateChess(ChessDispenser.DispenseChess()).GetComponent<Chess>());
        }
    }
}
