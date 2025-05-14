using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

public class Selector
{
    private float ChessSelectorLength_;
    private List<Chess> chessList_;
    private KeyValuePair<Chess, int> previewChess_ = new KeyValuePair<Chess, int>(null, -1);

    public Selector(List<Chess> chessList, float ChessSelectorLength = 50) {
        chessList_ = chessList;
        ChessSelectorLength_ = ChessSelectorLength;
    }

    public List<Chess> GetChesses() {
        return chessList_;
    }

    public bool PushBack(Chess chess) {
        if (chess == null) {
            return false;
        }
        chessList_.Add(chess);
        ResetAllChessPos();
        return true;
    }

    public bool Remove(string chessCode) {
        Chess chessOne = null;
        foreach(var chess in chessList_) {
            if (chess.GetChessProperty().CardCode == chessCode) {
                chessOne = chess;
                break;
            }
        }
        if (chessOne == null) {
            return false;
        }
        bool result = chessList_.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public bool RemoveByIndex(int index) {
        if (index < 0 || index >= chessList_.Count) {
            return false;
        }
        chessList_.RemoveAt(index);
        ResetAllChessPos();
        return true;
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

    public void Commit() {
        previewChess_ = new KeyValuePair<Chess, int>(null, 0);
        ResetAllChessPos();
    }

    public bool Preview(int index) {
        CancelPreview();
        previewChess_ = new KeyValuePair<Chess, int>(chessList_[index], index);
        bool result = chessList_.Remove(chessList_[index]);
        ResetAllChessPos();
        return result;
    }

    public bool Preview(Chess chessOne) {
        CancelPreview();
        int index = chessList_.IndexOf(chessOne);
        previewChess_ = new KeyValuePair<Chess, int>(chessOne, index);
        return RemoveByIndex(index);
    }

    public void CancelPreview() {
        if (previewChess_.Key == null) {
            Log.TestLine("CancelPreview Nothing to do.");
            return;
        }
        chessList_.Insert(previewChess_.Value, previewChess_.Key);
        previewChess_ = new KeyValuePair<Chess, int>(null, -1);
        ResetAllChessPos();
    }

    public Chess GetChess(int index){
        if (index >= 0 && index < chessList_.Count) {
            return chessList_[index];
        } else {
            return null;
        }
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
            result.Add(chess.GetChessProperty().CardCode);
        }
        return result;
    }

    private void ResetAllChessPos() {
        float chessGap = ChessSelectorLength_ / (chessList_.Count + 1);
        for (int i = 0; i < chessList_.Count; i++) {
            chessList_[i].SetPos(new Float3D(0, - i * 0.1f, - i * chessGap));
        }
    }
}