using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public void UpdateGridCardLevel(int cardLevel, int effectLevel, ChessPosStatus posStatus) {
    //     if (levelText_ != null) {
    //         if(cardLevel + effectLevel > 0 && posStatus >= ChessPosStatus.OCCUPIED_FRIEND) {
    //             levelText_.GetComponent<TextMesh>().text = (cardLevel + effectLevel).ToString();
    //             if(posStatus == ChessPosStatus.OCCUPIED_FRIEND) {
    //                 levelText_.GetComponent<TextMesh>().color = Color.green;
    //             } else {
    //                 levelText_.GetComponent<TextMesh>().color = Color.red;
    //             }
    //         } else if (cardLevel == 0 && effectLevel != 0 &&posStatus < ChessPosStatus.OCCUPIED_FRIEND){
    //             if(effectLevel > 0)
    //             levelText_.GetComponent<TextMesh>().text = effectLevel > 0? "+" + effectLevel.ToString(): effectLevel.ToString();
    //             levelText_.GetComponent<TextMesh>().color = Color.gray;
    //         } else {
    //             levelText_.GetComponent<TextMesh>().text = "";
    //         }
    //     }
    // }

    // private static void UpdateScore(List<List<List<int>>> chessGridStatus) {
    //     for(int i = 0; i < GlobalScope.chessGridNameList_.Count; i++) {
    //         int playerScore = Rival.GetScoreInOneLine(chessGridStatus, i);
    //         int rivalScore = Rival.GetScoreInOneLine(Rival.GetChessPadStatusInRivalView(chessGridStatus), i);
    //         TextMesh playerScoreText = GlobalScope.GirdScoreCounters_[i].Item1.transform.Find("score").GetComponent<TextMesh>();
    //         TextMesh rivalScoreText = GlobalScope.GirdScoreCounters_[i].Item2.transform.Find("score").GetComponent<TextMesh>();
    //         playerScoreText.text = playerScore.ToString();
    //         rivalScoreText.text = rivalScore.ToString();
    //         if(playerScore > rivalScore) {
    //             playerScoreText.color = Color.yellow;
    //             rivalScoreText.color = Color.gray;
    //         } else if (playerScore < rivalScore){
    //             playerScoreText.color = Color.gray;
    //             rivalScoreText.color = Color.yellow;
    //         } else {
    //             playerScoreText.color = Color.gray;
    //             rivalScoreText.color = Color.gray;
    //         }
    //     }
    // }

    // public void PlayerMouseHover() {
    //     if(posStatus_ <= ChessPosStatus.OCCUPIED_FRIEND && PlayerOperation.preview) {
    //         // float duration = 1.0f;
    //         // float startAlpha = 0.2f;
    //         // float endAlpha = 0.5f;
    //         // float alpha = Mathf.Lerp(startAlpha, endAlpha, (Mathf.Sin(Time.time / duration * 2 * Mathf.PI) + 1.5f) / 4);
    //         // Log.test(((Mathf.Sin(Time.time / duration * 2 * Mathf.PI) + 1) / 2).ToSafeString());
    //         float alpha = (Mathf.Sin(Time.time / 1.5f * 2 * Mathf.PI) + 1.5f) / 4;
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;
    //         if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject) {
    //             gridPlane_.SetActive(true);
    //             if(PlayerOperation.currentChessObject != PlayerOperation.CHESSNULL && PlayerOperation.CheckIfInputVaild(gameObject, PlayerOperation.currentChessObject)) {
    //                 gridPlane_.GetComponent<MeshRenderer>().material.color = new Color(0.6627451f, 1, 0.6901961f, alpha);//0.5294118f
    //                 // if(ChessSelector.previewChess_.Key != null) {
    //                 //     ChessSelector.DoPreviewToChessGridPos(gameObject);
    //                 // }
    //                 GlobalScope.system_static_.GetComponent<GameManager>().DoPreviewToChessGridPos(gameObject);
    //             } else {
    //                 gridPlane_.GetComponent<MeshRenderer>().material.color = new Color(1, 0.3812995f, 0.03447914f, alpha);
    //                 GlobalScope.system_static_.GetComponent<GameManager>().CancelPreviewToChessGridPos();
    //             }
    //         } else {
    //             gridPlane_.SetActive(false);
    //         }
    //     } else {
    //         gridPlane_.SetActive(false);
    //     }

    // public void UpdateGridPosStatus(ChessPosStatus posStatus) {
    //     switch (posStatus) {
    //         case ChessPosStatus.LEVEL_ONE_FRIEND:
    //         case ChessPosStatus.LEVEL_ONE_ENEMY:
    //             levelModels_[0].SetActive(true);
    //             levelModels_[1].SetActive(false);
    //             levelModels_[2].SetActive(false);
    //             break;
    //         case ChessPosStatus.LEVEL_TWO_FRIEND:
    //         case ChessPosStatus.LEVEL_TWO_ENEMY:
    //             levelModels_[0].SetActive(false);
    //             levelModels_[1].SetActive(true);
    //             levelModels_[2].SetActive(false);
    //             break;
    //         case ChessPosStatus.LEVEL_THREE_FRIEND:
    //         case ChessPosStatus.LEVEL_THREE_ENEMY:
    //             levelModels_[0].SetActive(false);
    //             levelModels_[1].SetActive(false);
    //             levelModels_[2].SetActive(true);
    //             break;
    //         default: 
    //             levelModels_[0].SetActive(false);
    //             levelModels_[1].SetActive(false);
    //             levelModels_[2].SetActive(false);
    //             break;
    //     }
    //     foreach (Transform child in transform)
    //     {
    //         foreach (Transform childChild in child)
    //         {
    //             Renderer renderer = childChild.gameObject.GetComponent<Renderer>();
    //             if (posStatus < ChessPosStatus.EMPTY) {
    //                 renderer.material.color = Color.green;
    //             } else {
    //                 renderer.material.color = Color.red;
    //             }
    //         }
    //     }
    // }
}
