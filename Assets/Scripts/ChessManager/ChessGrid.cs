using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ChessGrid : MonoBehaviour
{
    public GlobalScope.ChessPosStatus posStatus = GlobalScope.ChessPosStatus.EMPTY;
    GameObject levelOne;
    GameObject levelTwo;
    GameObject levelThree;
    public Tuple<int, int> chessGridPos;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < GlobalScope.chessPositionNameList.GetLength(0) && chessGridPos == null; i++) {
            for(int j = 0; j < GlobalScope.chessPositionNameList.GetLength(1); j++) {
                if(gameObject.name == GlobalScope.chessPositionNameList[i, j]) {
                    chessGridPos = new Tuple<int, int>(i, j);
                    break;
                }
            }
        }
        levelOne = transform.Find("levelOne").gameObject;
        levelTwo = transform.Find("levelTwo").gameObject;
        levelThree = transform.Find("levelThree").gameObject;
        ShowPosLevel();
    }

    // Update is called once per frame
    void Update()
    {
        ShowPosLevel();
    }
    public void UpdateStatus() {

    }
    public int GetChessLevel() {
        int result = (int)posStatus % 10;
        if(result > 3) {
            return -1;
        }
        return result;
    }
    public void ShowPosLevel() {
        switch (posStatus) {
            case GlobalScope.ChessPosStatus.LEVEL_ONE_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_ONE_ENEMY:
                levelOne.SetActive(true);
                levelTwo.SetActive(false);
                levelThree.SetActive(false);
                break;
            case GlobalScope.ChessPosStatus.LEVEL_TWO_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_TWO_ENEMY:
                levelOne.SetActive(false);
                levelTwo.SetActive(true);
                levelThree.SetActive(false);
                break;
            case GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_THREE_ENEMY:
                levelOne.SetActive(false);
                levelTwo.SetActive(false);
                levelThree.SetActive(true);
                break;
            default: 
                levelOne.SetActive(false);
                levelTwo.SetActive(false);
                levelThree.SetActive(false);
                break;
        }
        foreach (Transform child in transform)
        {
            foreach (Transform childChild in child)
            {
                Renderer renderer = childChild.gameObject.GetComponent<Renderer>();
                if (posStatus < GlobalScope.ChessPosStatus.EMPTY) {
                    renderer.material.color = Color.green;
                } else {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
}
