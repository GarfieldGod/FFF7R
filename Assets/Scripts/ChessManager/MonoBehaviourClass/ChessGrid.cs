using System;
using System.Collections.Generic;
using UnityEngine;


public class ChessGrid : MonoBehaviour
{
    public static Dictionary<Int2D, Tuple<CardEffectsScope, CardEffectsType, List<List<int>>>> tasksLasting_;
    public ChessPosStatus posStatus_ = ChessPosStatus.EMPTY;
    public Int2D chessGridPos_;
    public int cardLevel_ = 0;
    private GameObject[] levelModels_ = new GameObject[3];
    public TextMesh levelText_;
    private GameObject gridPlane_;

    void Start()
    {
        chessGridPos_ = GlobalScope.InitGlobalScopeChessGridMap(gameObject);
        levelModels_[0] = transform.Find("levelOne").gameObject;
        levelModels_[1] = transform.Find("levelTwo").gameObject;
        levelModels_[2] = transform.Find("levelThree").gameObject;
        gridPlane_ = transform.Find("gridPlane").gameObject;
        GameObject levelText = transform.Find("text_level").gameObject;

        if(levelModels_[0] == null || levelModels_[1] == null || levelModels_[2] == null || gridPlane_ == null) {
            Log.test("ChessGrid: Can not found child.");
        }
        levelText.SetActive(true);
        levelText_ = levelText.gameObject.GetComponent<TextMesh>();
        gridPlane_.SetActive(false);
    }
    public int GetChessPosLevel() {
        int result = (int)posStatus_ % 10;
        if(result > 3) {
            return -1;
        }
        return result;
    }
    public void UpdateGridStatus(List<List<List<int>>> chessGirdStatus) {
        ChessPosStatus myChessStatus = (ChessPosStatus)chessGirdStatus[0][chessGridPos_.x][chessGridPos_.y];
        int MyCardLevel = chessGirdStatus[1][chessGridPos_.x][chessGridPos_.y];
        posStatus_ = myChessStatus;
        UpdateGridPosStatus(myChessStatus);
        UpdateGridCardLevel(MyCardLevel, myChessStatus);
    }
    public void PreviewGridStatus(List<List<List<int>>> chessGirdStatus, bool self) {
        ChessPosStatus myChessStatus = (ChessPosStatus)chessGirdStatus[0][chessGridPos_.x][chessGridPos_.y];
        int MyCardLevel = chessGirdStatus[1][chessGridPos_.x][chessGridPos_.y];
        if(self) {
            UpdateGridPosStatus(ChessPosStatus.EMPTY);
        } else {
            UpdateGridPosStatus(myChessStatus);
        }
        UpdateGridCardLevel(MyCardLevel, myChessStatus);
    }
    public void UpdateGridPosStatus(ChessPosStatus posStatus) {
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
        if (levelText_ != null) {
            if(cardLevel > 0 && posStatus >= ChessPosStatus.OCCUPIED_FRIEND) {
                levelText_.GetComponent<TextMesh>().text = cardLevel.ToString();
                if(posStatus == ChessPosStatus.OCCUPIED_FRIEND) {
                    levelText_.GetComponent<TextMesh>().color = Color.green;
                } else {
                    levelText_.GetComponent<TextMesh>().color = Color.red;
                }
            } else {
                levelText_.GetComponent<TextMesh>().text = "";
            }
        }
    }
    public void PlayerMouseHover() {
        if(posStatus_ <= ChessPosStatus.OCCUPIED_FRIEND && PlayerOperation.preview) {
            // float duration = 1.0f;
            // float startAlpha = 0.2f;
            // float endAlpha = 0.5f;
            // float alpha = Mathf.Lerp(startAlpha, endAlpha, (Mathf.Sin(Time.time / duration * 2 * Mathf.PI) + 1.5f) / 4);
            // Log.test(((Mathf.Sin(Time.time / duration * 2 * Mathf.PI) + 1) / 2).ToSafeString());
            float alpha = (Mathf.Sin(Time.time / 1.5f * 2 * Mathf.PI) + 1.5f) / 4;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject) {
                gridPlane_.SetActive(true);
                if(PlayerOperation.currentChessObject != PlayerOperation.CHESSNULL && PlayerOperation.CheckIfInputVaild(gameObject, PlayerOperation.currentChessObject)) {
                    gridPlane_.GetComponent<MeshRenderer>().material.color = new Color(0.6627451f, 1, 0.6901961f, alpha);//0.5294118f
                    if(ChessSelector.previewChess_.Key != null) {
                        ChessSelector.DoPreviewToChessGridPos(gameObject);
                    }
                } else {
                    gridPlane_.GetComponent<MeshRenderer>().material.color = new Color(1, 0.3812995f, 0.03447914f, alpha);
                    ChessSelector.CancelPreviewToChessGridPos();
                }
            } else {
                gridPlane_.SetActive(false);
            }
        } else {
            gridPlane_.SetActive(false);
        }
    }
    void Update()
    {
        PlayerMouseHover();
    }
}
