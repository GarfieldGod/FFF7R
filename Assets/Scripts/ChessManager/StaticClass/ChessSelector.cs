using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChessSelector : MonoBehaviour
{
    public static List<Chess> chessList_ = new List<Chess>{};
    public GameObject previewPos;
    private static float ChessSelectorLength_ = 50;
    private static Vector3 previewPos_;
    private static Vector3 previewChessNowPos_;
    public static KeyValuePair<GameObject, int> previewChess_ = new KeyValuePair<GameObject, int>(null, 0);
    public static void PushBackChess(Chess chessOne) {
        if(chessOne == null) {
            return;
        }
        chessList_.Add(chessOne);
        ResetAllChessPos();
    }
    public static bool RemoveChess(GameObject chessObj) {
        bool result = chessList_.Remove(chessObj.GetComponent<Chess>());
        Destroy(chessObj);
        ResetAllChessPos();
        return result;
    }
    public static bool DoPreviewToPreViewPos(Chess chessOne) {
        previewChess_ = new KeyValuePair<GameObject, int>(chessOne.gameObject, chessList_.IndexOf(chessOne));
        bool result = chessList_.Remove(chessOne);
        if (result) {
            previewChessNowPos_ = previewPos_;
            previewChess_.Key.GetComponent<Chess>().enabled = false;
            previewChess_.Key.GetComponent<BoxCollider>().enabled = false;
            ResetAllChessPos();
        }
        return result;
    }
    public static void CancelPreview() {
        PlayerOperation.preview = false;
        previewChess_.Key.transform.localScale = Vector3.one;
        previewChessNowPos_ = new Vector3(0, 0, 0);
        if (previewChess_.Key == null) {
            Debug.Log("CancelPreview Nothing to do.");
            return;
        }
        chessList_.Insert(previewChess_.Value, previewChess_.Key.GetComponent<Chess>());
        previewChess_.Key.GetComponent<Chess>().enabled = true;
        previewChess_.Key.GetComponent<BoxCollider>().enabled = true;
        previewChess_ = new KeyValuePair<GameObject, int>(null, 0);
        ResetAllChessPos();
        GameManager.CommitChessStatusToChessPad();
    }
    public static bool DoPreviewToChessGridPos(GameObject chessGridObj) {
        if(previewChess_.Key == null) {
            return false;
        }
        previewChessNowPos_ = new Vector3(chessGridObj.transform.position.x, chessGridObj.transform.position.y - 1.5f, chessGridObj.transform.position.z + 0.5f);
        previewChess_.Key.transform.localScale = new Vector3(0.8f, 1, 0.8f);
        return true;
    }
    public static bool CancelPreviewToChessGridPos() {
        if(previewChess_.Key == null) {
            return false;
        }
        previewChessNowPos_ = previewPos_;
        previewChess_.Key.transform.localScale = Vector3.one;
        // MoveToLocal(previewChess_.Key, previewPos_, Quaternion.identity, 100f , true);
        return true;
    }
    public static void ResetAllChessPos() {
        float chessGap = ChessSelectorLength_ / (chessList_.Count + 1);
        for (int i = 0; i < chessList_.Count; i++) {
            chessList_[i].chessPos = new Vector3(0, - i * 0.1f, - i * chessGap);
        }
    }
// -----------------------------------------------------------------------------------Movement
    void Start() {
        previewPos_ = previewPos.transform.position;
    }
    void Update () {
        PreviewChessToPreviewPos();
    }
    private void PreviewChessToPreviewPos(){
        if(PlayerOperation.preview && previewChess_.Key != null) {
            previewChess_.Key.transform.position = Vector3.MoveTowards(previewChess_.Key.transform.position, previewChessNowPos_, 100f * Time.deltaTime);
            previewChess_.Key.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }
    // private static List<Tuple<GameObject, Vector3, Quaternion, float>> moveListLocal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    // private static List<Tuple<GameObject, Vector3, Quaternion, float>> moveListGlobal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    // public static void MoveToLocal(GameObject obj, Vector3 targetPosition, Quaternion rotation, float moveSpeed, bool isClearTaskList = false)
    // {
    //     if(isClearTaskList) {
    //         moveListLocal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    //     }
    //     moveListLocal.Add(new Tuple<GameObject, Vector3, Quaternion, float>(obj, targetPosition, rotation, moveSpeed));
    // }
    // public static void MoveToGlobal(GameObject obj, Vector3 targetPosition, Quaternion rotation, float moveSpeed, bool isClearTaskList = false)
    // {
    //     if(isClearTaskList) {
    //         moveListGlobal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    //     }
    //     moveListGlobal.Add(new Tuple<GameObject, Vector3, Quaternion, float>(obj, targetPosition, rotation, moveSpeed));
    // }
    // public static void ClearAllMoveMovement () {
    //     moveListGlobal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    //     moveListLocal = new List<Tuple<GameObject, Vector3, Quaternion, float>>{};
    // }
    // public static void GlobalScopeMovement() {
    //     if (moveListLocal.Count != 0) {
    //         for (int i = 0; i < moveListLocal.Count; i++)
    //         {
    //             var task = moveListLocal[i];
    //             if (task.Item1 != null && task.Item1.transform.localPosition != task.Item2)
    //             {
    //                 task.Item1.transform.localPosition = Vector3.MoveTowards(task.Item1.transform.localPosition, task.Item2, task.Item4 * Time.deltaTime);
    //                 task.Item1.transform.localRotation = task.Item3;
    //             } else {
    //                 moveListLocal.RemoveAt(i);
    //                 i--;
    //             }
    //         }
    //     }
    //     if (moveListGlobal.Count != 0) {
    //         for (int i = 0; i < moveListGlobal.Count; i++)
    //         {
    //             var task = moveListGlobal[i];
    //             if (task.Item1.transform.position != task.Item2)
    //             {
    //                 task.Item1.transform.position = Vector3.MoveTowards(task.Item1.transform.position, task.Item2, task.Item4 * Time.deltaTime);
    //                 task.Item1.transform.rotation = task.Item3;
    //             } else {
    //                 moveListGlobal.RemoveAt(i);
    //                 i--;
    //             }
    //         }
    //     }
    // }
}