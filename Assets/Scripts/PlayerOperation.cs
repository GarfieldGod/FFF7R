using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOperation : MonoBehaviour
{
    public static GameObject currentChessObject = null;
    public static GameObject CHESSNULL;
    public static bool preview = false;
    void Start()
    {
        CHESSNULL = transform.Find("chessNull").gameObject;
        currentChessObject = CHESSNULL;
    }

    void Update()
    {
        PlayerClick();
        PlayerPreView();
        // ScreenPosToWorldPos(Input.mousePosition);
    }

    // UnityEngine.Vector3 ScreenPosToWorldPos(UnityEngine.Vector3 mouseScreenPosition){
    //     return Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane));
    // }

    void PlayerClick() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                GameObject hitOne = hit.collider.gameObject;
                if (GlobalScope.chessNameSet.Contains(hitOne.name)) {
                    PlyerDoSelect(hitOne);
                }
                if (currentChessObject != CHESSNULL && GlobalScope.chessPositionNameSet.Contains(hitOne.name)) {
                    PlayerDoInput(hitOne);
                }
            }
        } else if (Input.GetMouseButtonDown(1)){
            if(preview) {
                GetComponent<GameManager>().CancelPreview();
                currentChessObject = CHESSNULL;
                preview = false;
            }
        }
    }

    void PlayerPreView() {
        if (preview == true) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit) && GlobalScope.chessPositionNameSet.Contains(hit.collider.gameObject.name)) {
                GameObject hitOne = hit.collider.gameObject;
                if (GetComponent<GameManager>().PlayerTurn && CheckIfInputVaild(hitOne, currentChessObject)) {
                    ChessInputParmObj parmsInput = new ChessInputParmObj(
                        hitOne,
                        currentChessObject,
                        ChessPad.chessPadInfo_
                    );
                    Int2D chessGridPos = hitOne.GetComponent<ChessGrid>().chessGridPos_;
                    List<List<List<int>>> chessGridStatusTemp = ChessInputer.PreviewChessGridStatusByObj(parmsInput);
                    ChessPad.PreviewStatusToChessPad(chessGridStatusTemp, chessGridPos);
                } else {
                    ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
                }
            } else {
                ChessPad.CommitChessPadInfoToChessPad(ChessPad.chessPadInfo_);
                GetComponent<GameManager>().CancelPreviewToChessGridPos();
            }
        }
    }

    void PlayerDoInput(GameObject chessGridToInput) {
        if (CheckIfInputVaild(chessGridToInput, currentChessObject) && GetComponent<GameManager>().PlayerTurn) {
            Debug.Log("Input is vaild.");
            ChessInputParmObj parmsInput = new ChessInputParmObj(
                chessGridToInput,
                currentChessObject,
                ChessPad.chessPadInfo_
            );
            GetComponent<GameManager>().DoPlayerTurn(parmsInput);
            currentChessObject = CHESSNULL;
            preview = false;
        } else  {
            Debug.Log("Input is Invaild.");
        }
    }

    void PlyerDoSelect(GameObject chessToSelect){
        if (CheckIfSelectVaild(chessToSelect)  && GetComponent<GameManager>().PlayerTurn) {
            Debug.Log("Select is vaild.");
            if(currentChessObject != CHESSNULL) {
                GetComponent<GameManager>().CancelPreview();
                currentChessObject = CHESSNULL;
            }
            currentChessObject = chessToSelect;
            GetComponent<GameManager>().DoPreviewToPreViewPos(chessToSelect.GetComponent<Chess>());
            preview = true;
        } else {
            Debug.Log("Select is Invaild.");
        }
    }

    bool CheckIfSelectVaild (GameObject chessObject) {
        // if (GlobalScope.GetChessProperty(chessObject.name).Level > highestLevel) {
        //     return false;
        // }
        return true;
    }

    public static bool CheckIfInputVaild (GameObject chessPositionObject, GameObject chessObject) {
        ChessGrid chessPos = chessPositionObject.GetComponent<ChessGrid>();
        int chessPosLevel = chessPos.GetChessPosLevel();
        if (chessPosLevel <= 0 || chessPosLevel < GlobalScope.GetChessProperty(chessObject.name).Cost) {
            return false;
        }
        List<Tuple<Int2D, int>> vaildChessGrids = Rival.GetAllVaildChessGrids(ChessPad.chessPadInfo_.chessPadStatus[0]);
        foreach(var vaildChessGrid in vaildChessGrids) {
            if(chessPos.chessGridPos_.x == vaildChessGrid.Item1.x && chessPos.chessGridPos_.y == vaildChessGrid.Item1.y) {
                return true;
            }
        }
        return false;
    }
}
