using System;
using System.Collections.Generic;

public class Dispenser
{
    public List<Chess> ChessPool => chessPool_;
    public int Count => chessPool_.Count;
    private List<Chess> chessPool_;

    public Dispenser(List<Chess> chessPool){
        chessPool_ = chessPool;
    }

    public Chess this[int index]
    {
        get
        {
            if (index < 0 || index >= chessPool_.Count)
            {
                return null;
            }
            return chessPool_[index];
        }
        set
        {
            chessPool_[index] = value;
        }
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
}
