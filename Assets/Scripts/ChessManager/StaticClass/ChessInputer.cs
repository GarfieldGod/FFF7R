using System.Collections.Generic;


#if UNITY_ENGINE
using UnityEngine;
public struct ChessInputParms {
    public Int2D pos;
    public string cardCode;
    public ChessInputParms(Int2D pos, string cardCode) {
        this.pos = pos;
        this.cardCode = cardCode;
    }
    public bool Empty() {
        if(cardCode == null) {
            return true;
        }
        return false;
    }
}
public struct ChessInputParmObj {
    public GameObject chessGrid;
    public GameObject chessObj;
    public ChessPadInfo chessPadInfo;
    public ChessInputParmObj(GameObject chessGrid, GameObject chessObj, ChessPadInfo chessPadInfo) {
        this.chessGrid = chessGrid;
        this.chessObj = chessObj;
        this.chessPadInfo = chessPadInfo;
    }
}
public struct ChessInputParm {
    public ChessInputParms chessInputParms;
    public ChessPadInfo chessPadInfo;
    public ChessInputParm(ChessInputParms chessInputParms, ChessPadInfo chessPadInfo) {
        this.chessInputParms = chessInputParms;
        this.chessPadInfo = chessPadInfo;
    }
}
public class ChessInputer : MonoBehaviour {
    public ChessInputer(List<string> ChessPool, int chessSelectorInitSize, ChessPadInfo chessPadInfo, InputerType inputerType = InputerType.PLAYER) {
        chessSelector_ = new ChessSelector(GlobalScope.previewPos_static_);
        if(inputerType == InputerType.PLAYER) {
            chessDispenser_ = new ChessDispenser(chessSelector_, ChessPool, GlobalScope.chessSelector_static_);
        } else {
            chessDispenser_ = new ChessDispenser(chessSelector_, ChessPool, GlobalScope.chessSelector_rival_static_);
        }
        effectsMaker_ = new EffectsMaker(chessPadInfo.chessPadStatus[0], inputerType);
        inputerType_ = inputerType;
        for( int i = 0 ; i < chessSelectorInitSize ; i++ ) {
            chessDispenser_.DispenserNewChessToChessSelector();
        }
    }
    private ChessSelector chessSelector_;
    private ChessDispenser chessDispenser_;
    private EffectsMaker effectsMaker_;
    private InputerType inputerType_;
    public void GetChessInput(GameObject chessGrid, GameObject chessObj, ChessPadInfo chessPadInfo) {
        Int2D chessGridPos = chessGrid.GetComponent<ChessGrid>().chessGridPos_;
        ChessProperty property = GlobalScope.GetChessProperty(chessObj.name);
        GetChessInput(chessGridPos, chessObj.name, chessPadInfo, false);
        chessSelector_.RemoveChess(chessObj.GetComponent<Chess>());
    }
    public void GetChessInput(Int2D chessGridPos, string cardCode, ChessPadInfo chessPadInfo, bool removeCard = true) {
        if(inputerType_ >= InputerType.RIVAL) {
            Rival.TurnChessPadStatusToRivalView(chessPadInfo.chessPadStatus);
        }
        ChessProperty property = GlobalScope.GetChessProperty(cardCode);
        chessPadInfo.chessPadStatus[0] = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessPadInfo.chessPadStatus[0]);
        chessPadInfo.chessPadStatus[1][chessGridPos.x][chessGridPos.y] = property.Level;
        // chessPadInfo.chessPadStatus[2] = CardEffect.DoCardEffect(chessGridPos, property, chessPadInfo);
        if(inputerType_ >= InputerType.RIVAL) {
            Rival.TurnChessPadStatusToRivalView(chessPadInfo.chessPadStatus);
        }

        GetCardModelOn(chessGridPos, property);
        if(removeCard) {
            chessSelector_.RemoveChess(cardCode);
        }
        chessDispenser_.DispenserNewChessToChessSelector();
    }
    private void GetCardModelOn(Int2D chessGridPos, ChessProperty property) { // TODO somebug in rival view
        Int2D finalPos = chessGridPos;
        if(inputerType_ >= InputerType.RIVAL) {
            finalPos = Rival.GetChessGridPosInRivalView(finalPos);
        }
        GameObject chessGridObj = GlobalScope.GetChessGridObjectByChessGridPos(finalPos);
        GameObject instantiateModel = chessDispenser_.InstantiateChessModel(GlobalScope.chessModelPrefab_static_, chessGridObj, property, true);
        instantiateModel.name = "CardModel";
        instantiateModel.transform.localPosition = new Vector3(0.5f, -0.8f, 0);
        instantiateModel.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        instantiateModel.transform.localScale = new Vector3(90, 80, 150);
    }
    public static List<List<List<int>>> PreviewChessGridStatusByObj(ChessInputParmObj parms) {
        Int2D chessGridPos = parms.chessGrid.GetComponent<ChessGrid>().chessGridPos_;
        ChessProperty property = GlobalScope.GetChessProperty(parms.chessObj.name);
        return PreviewChessGridStatus(chessGridPos, property, parms.chessPadInfo);
    }
    public static List<List<List<int>>> PreviewChessGridStatusByPos(ChessInputParm parms) {
        ChessProperty property = GlobalScope.GetChessProperty(parms.chessInputParms.cardCode);
        return PreviewChessGridStatus(parms.chessInputParms.pos, property, parms.chessPadInfo);
    }
    public static List<List<List<int>>> PreviewChessGridStatus(Int2D chessGridPos, ChessProperty property, ChessPadInfo chessPadInfo) {
        List<List<List<int>>> chessGridStatusTemp = GlobalScope.DeepCopy3DList(chessPadInfo.chessPadStatus);
        ChessPadInfo chessPadInfoTemp = new ChessPadInfo(chessGridStatusTemp, null);
        chessGridStatusTemp[0] = PosEffect.DoPosEffect(chessGridPos, property.PosEffects, chessGridStatusTemp[0]);
        chessGridStatusTemp[1][chessGridPos.x][chessGridPos.y] = property.Level;
        // chessGridStatusTemp[2] = CardEffect.DoCardEffect(chessGridPos, property, chessPadInfoTemp);
        return chessGridStatusTemp;
    }
    public bool IfCanDoInput() {
        if(chessSelector_.GetChessNumInChessInHand() != 0) {
            return true;
        }
        return false;
    }
    public List<string> GetChessInChessPool() {
        return chessDispenser_.GetChessInChessPool();
    }
    public List<string> GetChessInChessInHand() {
        return chessSelector_.GetChessInChessInHand();
    }
    public List<string> GetAllVailChess(){
        List<string> result = GetChessInChessInHand();
        foreach(var str in GetChessInChessPool()) {
            result.Add(str);
        }
        return result;
    }
    public ChessSelector GetChessSelector() {
        return chessSelector_;
    }
}
#endif

public enum InputerType {
    PLAYER,
    RIVAL,
    Ai
}