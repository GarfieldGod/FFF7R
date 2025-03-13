using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ChessGrid : MonoBehaviour
{
    public ChessPosStatus posStatus_ = ChessPosStatus.EMPTY;
    public Int2D chessGridPos_;
    public int cardLevel_ = 0;
    private GameObject[] levelModels_ = new GameObject[3];
    private TextMesh levelText_;

    // Start is called before the first frame update
    void Awake()
    {
        for(int i = 0; i < GlobalScope.chessPositionNameList.GetLength(0); i++) {
            for(int j = 0; j < GlobalScope.chessPositionNameList.GetLength(1); j++) {
                if(gameObject.name == GlobalScope.chessPositionNameList[i, j]) {
                    chessGridPos_ = new Int2D(i, j);
                    break;
                }
            }
        }
        levelModels_[0] = transform.Find("levelOne").gameObject;
        levelModels_[1] = transform.Find("levelTwo").gameObject;
        levelModels_[2] = transform.Find("levelThree").gameObject;
        GameObject levelText = transform.Find("text_level").gameObject;
        if(levelModels_[0] == null || levelModels_[1] == null || levelModels_[2] == null || levelText == null) {
            Log.test("ChessGrid: Can not found child.");
        }
        levelText.SetActive(true);
        levelText_ = levelText.gameObject.GetComponent<TextMesh>();
    }
    void Update()
    {
        // UpdateGridPosStatus(posStatus_, cardLevel_, "");
    }
    public int GetChessPosLevel() {
        int result = (int)posStatus_ % 10;
        if(result > 3) {
            return -1;
        }
        return result;
    }
    public void UpdateGridPosStatus(ChessPosStatus posStatus, int cardLevel, string cardName) {
        UpdateGridPosStatus(posStatus);
        UpdateGridCardLevel(cardLevel, posStatus);
        GetCardModelOn(cardName);
    }
    public void UpdateGridPosStatus(ChessPosStatus posStatus) {
        posStatus_ = posStatus;
        switch (posStatus) {
            case ChessPosStatus.LEVEL_ONE_FRIEND:
            case ChessPosStatus.LEVEL_ONE_ENEMY:
                levelModels_[0].SetActive(true);
                levelModels_[1].SetActive(false);
                levelModels_[2].SetActive(false);
                break;
            case ChessPosStatus.LEVEL_TWO_FRIEND:
            case ChessPosStatus.LEVEL_TWO_ENEMY:
                levelModels_[0].SetActive(false);
                levelModels_[1].SetActive(true);
                levelModels_[2].SetActive(false);
                break;
            case ChessPosStatus.LEVEL_THREE_FRIEND:
            case ChessPosStatus.LEVEL_THREE_ENEMY:
                levelModels_[0].SetActive(false);
                levelModels_[1].SetActive(false);
                levelModels_[2].SetActive(true);
                break;
            default: 
                levelModels_[0].SetActive(false);
                levelModels_[1].SetActive(false);
                levelModels_[2].SetActive(false);
                break;
        }
        foreach (Transform child in transform)
        {
            foreach (Transform childChild in child)
            {
                Renderer renderer = childChild.gameObject.GetComponent<Renderer>();
                if (posStatus < ChessPosStatus.EMPTY) {
                    renderer.material.color = Color.green;
                } else {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
    public void UpdateGridCardLevel(int cardLevel, ChessPosStatus posStatus) {
        if(cardLevel > 0 && posStatus >= ChessPosStatus.OCCUPIED_FRIEND) {
            levelText_.GetComponent<TextMesh>().text = cardLevel.ToString();
            if(posStatus == ChessPosStatus.OCCUPIED_FRIEND) {
                levelText_.GetComponent<TextMesh>().color = Color.black;
            } else {
                levelText_.GetComponent<TextMesh>().color = Color.red;
            }
        } else {
            levelText_.GetComponent<TextMesh>().text = "";
        }
    }
    public void GetCardModelOn(string cardName) {
    }
}
