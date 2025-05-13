using System;
using System.Collections.Generic;

public class Selector
{
    private float ChessSelectorLength_;
    private List<Chess> chessList_;
    private KeyValuePair<Chess, int> previewChess_ = new KeyValuePair<Chess, int>(null, -1);

    public Selector(List<Chess> chessList, float ChessSelectorLength = 50) {
        chessList_ = chessList;
        ChessSelectorLength_ = ChessSelectorLength;
    }

    public bool PushBack(Chess chess) {
        if(chess == null) {
            return false;
        }
        chessList_.Add(chess);
        ResetAllChessPos();
        return true;
    }

    public bool Remove(string chessCode) {
        Chess chessOne = null;
        foreach(var chess in chessList_) {
            if(chess.cardCode_ == chessCode) {
                chessOne = chess;
                break;
            }
        }
        if(chessOne == null) {
            return false;
        }
        bool result = chessList_.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public void RemoveByIndex(int index) {
        chessList_.RemoveAt(index);
        ResetAllChessPos();
    }

    public bool Remove(List<int> indexs) {
        List<Chess> popList = new List<Chess>{};
        foreach(int index in indexs) {
            popList.Add(chessList_[index]);
        }
        bool result = true;
        foreach(Chess chess in popList) {
            if (!chessList_.Remove(chess)) {
                result = false;
            }
        }
        ResetAllChessPos();
        return result;
    }

    public bool Remove(Chess chessOne) {
        bool result = chessList_.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public bool Preview(Chess chessOne) {
        previewChess_ = new KeyValuePair<Chess, int>(chessOne, chessList_.IndexOf(chessOne));
        bool result = chessList_.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public void CancelPreview() {
        if (previewChess_.Key == null) {
            Log.test("CancelPreview Nothing to do.");
            return;
        }
        chessList_.Insert(previewChess_.Value, previewChess_.Key);
        previewChess_ = new KeyValuePair<Chess, int>(null, -1);
        ResetAllChessPos();
    }

    public List<Chess> GetAllChess(){
        return chessList_;
    }

    public int GetAllChessNum(){
        return chessList_.Count;
    }

    public List<string> GetAllChessCode() {
        List<string> result = new List<string>();
        foreach(var chess in chessList_) {
            result.Add(chess.cardCode_);
        }
        return result;
    }

    private void ResetAllChessPos() {
        float chessGap = ChessSelectorLength_ / (chessList_.Count + 1);
        for (int i = 0; i < chessList_.Count; i++) {
            chessList_[i].chessPos = new Float3D(0, - i * 0.1f, - i * chessGap);
        }
    }
}