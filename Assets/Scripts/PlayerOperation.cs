using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerOperation : MonoBehaviour
{
    public GameObject currentChessObject = null;
    public GameObject CHESSNULL;
    public int highestLevel = 1;
    // Start is called before the first frame update
    public bool preview = false;
    void Start()
    {
        currentChessObject = CHESSNULL;
    }

    // Update is called once per frame
    void Update()
    {
        playerClick();
        ScreenPosToWorldPos(Input.mousePosition);
    }

    UnityEngine.Vector3 ScreenPosToWorldPos(UnityEngine.Vector3 mouseScreenPosition){
        return Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane));
    }

    void playerClick() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                GameObject hitOne = hit.collider.gameObject;
                //Debug.Log("Hit Object: " + hitOne.name);
                //Select Chess
                if (GlobalScope.chessNameSet.Contains(hitOne.name)) {
                    if (CheckIfSelectVaild(hitOne)) {
                        Debug.Log("Select is vaild.");
                        if(currentChessObject != CHESSNULL) {
                            ChessSelector.CancelPreview();
                            currentChessObject = CHESSNULL;
                        }
                        currentChessObject = hitOne;
                        ChessSelector.DoPreview(hitOne.GetComponent<Chess>());
                        preview = true;
                    } else {
                        Debug.Log("Select is Invaild.");
                    }
                }
                //Input Chess
                if (currentChessObject != CHESSNULL && GlobalScope.chessPositionNameSet.Contains(hitOne.name)) {
                    if (CheckIfInputVaild(hitOne, currentChessObject)) {
                        Debug.Log("Input is vaild.");
                        GetComponent<ChessInputer>().GetChessInput(hitOne, currentChessObject);
                        GameObject newChessObj = GetComponent<ChessDispenser>().InstantiateChess(ChessDispenser.DispenseChess());
                        if (newChessObj != null) {
                        ChessSelector.PushBackChess(newChessObj.GetComponent<Chess>());
                        }
                        currentChessObject = CHESSNULL;
                        GetComponent<GameManager>().GameTurns++;
                    } else  {
                        Debug.Log("Input is Invaild.");
                    }
                }
            }
        } else if (Input.GetMouseButtonDown(1)){
            if(preview) {
                ChessSelector.CancelPreview();
                currentChessObject = CHESSNULL;
            }
        }
    }

    bool CheckIfSelectVaild (GameObject chessObject) {
        if (GlobalScope.GetChessProperty(chessObject.name).Level > highestLevel) {
            return false;
        }
        return true;
    }

    bool CheckIfInputVaild (GameObject chessPositionObject, GameObject chessObject) {
        ChessGrid chessPos = chessPositionObject.GetComponent<ChessGrid>();
        int chessPosLevel = chessPos.GetChessLevel();
        if (chessPosLevel <= 0 || chessPosLevel < GlobalScope.GetChessProperty(chessObject.name).Level) {
            return false;
        }
        if(!GetComponent<GameManager>().PlayerTurn) {
            return false;
        }
        List<Tuple<Tuple<int, int>, int>> vaildChessGrids = Rival.GetAllVaildChessGrids(GlobalScope.chessGridStatus);
        foreach(var vaildChessGrid in vaildChessGrids) {
            if(chessPos.chessGridPos.Item1 == vaildChessGrid.Item1.Item1 && chessPos.chessGridPos.Item2 == vaildChessGrid.Item1.Item2) {
                return true;
            }
        }
        return false;
    }
}
