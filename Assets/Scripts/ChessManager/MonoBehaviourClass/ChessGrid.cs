using System;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEngine;

public class ChessGrid : MonoBehaviour
{
    public ChessPosStatus posStatus_ = ChessPosStatus.EMPTY;
    public ChessPosStatus posStatusTemp_ = ChessPosStatus.EMPTY;
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
    public void UpdateGridStatus(List<List<List<int>>> chessGirdStatus, bool commit, bool clearSelf)
    {
        ChessPosStatus myChessStatus = (ChessPosStatus)chessGirdStatus[0][chessGridPos_.x][chessGridPos_.y];
        int cardLevel = chessGirdStatus[1][chessGridPos_.x][chessGridPos_.y];
        int effectLevel = chessGirdStatus[2][chessGridPos_.x][chessGridPos_.y];
        if(cardLevel + effectLevel <= 0  && myChessStatus >= ChessPosStatus.OCCUPIED_FRIEND && commit) {
            ResetChessGrid(chessGirdStatus);
            myChessStatus = posStatusTemp_;
        }

        if(myChessStatus >= ChessPosStatus.OCCUPIED_FRIEND && posStatus_ < ChessPosStatus.OCCUPIED_FRIEND) {
            posStatusTemp_ = posStatus_;
        }
        if (commit) {
            posStatus_ = myChessStatus;
        } else if (clearSelf) {
            myChessStatus = ChessPosStatus.EMPTY;
        }
        UpdateGridPosStatus(myChessStatus);

        UpdateGridCardLevel(cardLevel, effectLevel, myChessStatus);
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
    public void UpdateGridCardLevel(int cardLevel, int effectLevel, ChessPosStatus posStatus) {
        if (levelText_ != null) {
            if(cardLevel + effectLevel > 0 && posStatus >= ChessPosStatus.OCCUPIED_FRIEND) {
                levelText_.GetComponent<TextMesh>().text = (cardLevel + effectLevel).ToString();
                if(posStatus == ChessPosStatus.OCCUPIED_FRIEND) {
                    levelText_.GetComponent<TextMesh>().color = Color.green;
                } else {
                    levelText_.GetComponent<TextMesh>().color = Color.red;
                }
            } else if (cardLevel == 0 && effectLevel != 0 &&posStatus < ChessPosStatus.OCCUPIED_FRIEND){
                if(effectLevel > 0)
                levelText_.GetComponent<TextMesh>().text = effectLevel > 0? "+" + effectLevel.ToString(): effectLevel.ToString();
                levelText_.GetComponent<TextMesh>().color = Color.gray;
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
                    // if(ChessSelector.previewChess_.Key != null) {
                    //     ChessSelector.DoPreviewToChessGridPos(gameObject);
                    // }
                    GlobalScope.system_static_.GetComponent<GameManager>().DoPreviewToChessGridPos(gameObject);
                } else {
                    gridPlane_.GetComponent<MeshRenderer>().material.color = new Color(1, 0.3812995f, 0.03447914f, alpha);
                    GlobalScope.system_static_.GetComponent<GameManager>().CancelPreviewToChessGridPos();
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
    public void ResetChessGrid(List<List<List<int>>> chessGirdStatus) {
        chessGirdStatus[0][chessGridPos_.x][chessGridPos_.y] = (int)posStatusTemp_;
        chessGirdStatus[1][chessGridPos_.x][chessGridPos_.y] = 0;
        ChessPad.TriggerEnemyDeadDelayTask();
        GameObject cardModel = gameObject.transform.Find("CardModel").gameObject;
        if(cardModel != null) {
            Destroy(cardModel);
        }
        ChessPad.TriggerSelfDeadDelayTask(chessGridPos_);
        ChessPad.RemoveCardDelayEffect(chessGridPos_);
    }
    // public void OnSelfDead() {
    //     ChessProperty chessProperty = ChessPad.QueryDelayEffectsList(chessGridPos_, CardEffectsType.ON_SELF_DEAD);
    //     if (chessProperty != null) {
    //         ChessPad.chessPadInfo_.chessPadStatus[2] = CardEffect.DoDelayedCardEffect(chessGridPos_, chessProperty, ChessPad.chessPadInfo_);
    //     }
    // }
    // public void OnEnemyDead() {
    //     ChessProperty chessProperty = ChessPad.QueryDelayEffectsList(chessGridPos_, CardEffectsType.ON_ENEMY_DEAD);
    //     if (chessProperty != null) {
    //         ChessPad.chessPadInfo_.chessPadStatus[2] = CardEffect.DoDelayedCardEffect(chessGridPos_, chessProperty, ChessPad.chessPadInfo_);
    //     }
    // }
    // public void OnEnhanced() {
    //     ChessProperty chessProperty = ChessPad.QueryDelayEffectsList(chessGridPos_, CardEffectsType.ON_ENHANCED);
    //     if (chessProperty != null) {
    //         ChessPad.chessPadInfo_.chessPadStatus[2] = CardEffect.DoDelayedCardEffect(chessGridPos_, chessProperty, ChessPad.chessPadInfo_);
    //     }
    // }
}
//-------------------------------------------------------------------------------------------------static
public struct ChessPadInfo {
    public List<List<List<int>>> chessPadStatus;
    public Dictionary<Int2D, ChessProperty> delayEffectsList;
    public ChessPadInfo(List<List<List<int>>> chessPadStatus, Dictionary<Int2D, ChessProperty> delayEffectsList = null){
        this.chessPadStatus = chessPadStatus;
        this.delayEffectsList = delayEffectsList;
    }
}

public class ChessPad {
    public static ChessPadInfo chessPadInfo_;
    // private static Dictionary<Int2D, ChessProperty> DelayEffectsList_ = new Dictionary<Int2D, ChessProperty>();
    public static bool QueryDelayEffectsList(Int2D pos, CardEffectsType cardEffectsType) {
        if(chessPadInfo_.delayEffectsList.ContainsKey(pos) || chessPadInfo_.delayEffectsList[pos].CardEffects.Item2 == cardEffectsType) {
            return true;
        }
        return false;
    }
    public static bool QueryDelayEffectsList(Int2D pos) {
        if(chessPadInfo_.delayEffectsList.ContainsKey(pos)) {
            return true;
        }
        return false;
    }
    public static void RemoveCardDelayEffect(Int2D taskId) {
        if(chessPadInfo_.delayEffectsList.ContainsKey(taskId)) {
            chessPadInfo_.delayEffectsList.Remove(taskId);
        }
    }
    public static void TriggerSelfDeadDelayTask(Int2D taskId) {
        if( chessPadInfo_.delayEffectsList.Count == 0 ) return;
        if (QueryDelayEffectsList(taskId)) {
            // chessPadInfo_.chessPadStatus[2] = CardEffect.DoDelayedCardEffect(taskId, chessPadInfo_.delayEffectsList[taskId], chessPadInfo_);
        }
    }
    public static void TriggerEnemyDeadDelayTask() {
        if( chessPadInfo_.delayEffectsList.Count == 0 ) return;
        foreach(var task in chessPadInfo_.delayEffectsList) {
            if(task.Value.CardEffects.Item2 == CardEffectsType.ON_ENEMY_DEAD) {
                DoCardEffectTask(task.Key);
            }
        }
    }
    public static void DoCardEffectTask(Int2D taskId) {
        // chessPadInfo_.chessPadStatus[2] = CardEffect.DoDelayedCardEffect(taskId, chessPadInfo_.delayEffectsList[taskId], chessPadInfo_);
    }
    public static void CommitChessPadInfoToChessPad(ChessPadInfo chessPadInfo){
        List<GameObject> chessGrids = GlobalScope.GetAllChessGrid();
        foreach(var chessGridObj in chessGrids) {
            ChessGrid chessGrid = chessGridObj.GetComponent<ChessGrid>();
            chessGrid.UpdateGridStatus(chessPadInfo.chessPadStatus, true, false);
        }
        UpdateScore(chessPadInfo.chessPadStatus);
    }
    public static void PreviewStatusToChessPad(List<List<List<int>>> chessGridStatus, Int2D selfPos){
        List<GameObject> chessGrids = GlobalScope.GetAllChessGrid();
        foreach(var chessGridObj in chessGrids) {
            ChessGrid chessGrid = chessGridObj.GetComponent<ChessGrid>();
            chessGrid.UpdateGridStatus(chessGridStatus, false, chessGrid.chessGridPos_ == selfPos);
        }
        UpdateScore(chessGridStatus);
    }
    private static void UpdateScore(List<List<List<int>>> chessGridStatus) {
        for(int i = 0; i < GlobalScope.chessGridNameList_.Count; i++) {
            int playerScore = Rival.GetScoreInOneLine(chessGridStatus, i);
            int rivalScore = Rival.GetScoreInOneLine(Rival.GetChessPadStatusInRivalView(chessGridStatus), i);
            TextMesh playerScoreText = GlobalScope.GirdScoreCounters_[i].Item1.transform.Find("score").GetComponent<TextMesh>();
            TextMesh rivalScoreText = GlobalScope.GirdScoreCounters_[i].Item2.transform.Find("score").GetComponent<TextMesh>();
            playerScoreText.text = playerScore.ToString();
            rivalScoreText.text = rivalScore.ToString();
            if(playerScore > rivalScore) {
                playerScoreText.color = Color.yellow;
                rivalScoreText.color = Color.gray;
            } else if (playerScore < rivalScore){
                playerScoreText.color = Color.gray;
                rivalScoreText.color = Color.yellow;
            } else {
                playerScoreText.color = Color.gray;
                rivalScoreText.color = Color.gray;
            }
        }
    }
}