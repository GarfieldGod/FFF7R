using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;
using UnityEngine;


public class player_operate : MonoBehaviour
{
    public GameObject currentChessObject = null;
    public GameObject CHESSNULL;
    public GameObject chessPad;
    public int highestLevel = 1;
    // Start is called before the first frame update
    void Start()
    {
        currentChessObject = CHESSNULL;
    }

    // Update is called once per frame
    void Update()
    {
        playerClick();
    }

    void playerClick() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                GameObject hitOne = hit.collider.gameObject;
                Debug.Log("Hit Object: " + hitOne.name);
                //Select Chess
                if (GlobalScope.chessNameSet.Contains(hitOne.name)) {
                    if (CheckIfSelectVaild(hitOne)) {
                        Debug.Log("Select is vaild.");
                        currentChessObject = hitOne;
                    } else {
                        Debug.Log("Select is Invaild.");
                    }
                }
                //Input Chess
                if (currentChessObject != CHESSNULL && GlobalScope.chessPositionNameSet.Contains(hitOne.name)) {
                    if (CheckIfInputVaild(hitOne, currentChessObject)) {
                        Debug.Log("Input is vaild.");
                        chessPad.GetComponent<chess_pad>().GetChessInput(hitOne, currentChessObject.name);
                    } else  {
                        Debug.Log("Input is Invaild.");
                    }
                }
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
        chess_position chessPos = chessPositionObject.GetComponent<chess_position>();
        if (chessPos.posStatus >= GlobalScope.ChessPosStatus.EMPTY) {
            return false;
        }
        int chessPosLevel = chessPos.level;
        if (chessPosLevel <= 0 || chessPosLevel < GlobalScope.GetChessProperty(chessObject.name).Level) {
            return false;
        }
        return true;
    }

    void SelectChess() {
        Debug.Log("SelectChess.// TODO");
    }
}
