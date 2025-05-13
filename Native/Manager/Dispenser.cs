using System;
using System.Collections.Generic;

public class Dispenser
{
    public Dispenser(List<Chess> chessPool){
        chessPool_ = chessPool;
    }

    public Dispenser(List<string> chessName){
        List<Chess> chessPool = new List<Chess>{};
        foreach(string name in chessName) {
            ChessProperty chessProperty = Property.GetChessProperty(name);
            Chess chess = new Chess(chessProperty);
            chessPool.Add(chess);
        }
        chessPool_ = chessPool;
    }
    private List<Chess> chessPool_;

    public Chess Dispense() {
        if (chessPool_.Count == 0) {
            return null;
        }
        System.Random random = new System.Random();
        int randomIndex = random.Next(chessPool_.Count);
        Chess chess = chessPool_[randomIndex];
        chessPool_.RemoveAt(randomIndex);
        return chess;
    }

    public List<Chess> GetChessInChessPool() {
        return chessPool_;
    }
}
