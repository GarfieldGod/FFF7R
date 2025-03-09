using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ChessSelector : MonoBehaviour
{
    public static float ChessSelectorLength = 60;
    public static List<Chess> chessList = new List<Chess>{};
    public static Vector3 previewPos = new Vector3(22, -5, 3);
    public static KeyValuePair<GameObject, int> previewChess = new KeyValuePair<GameObject, int>(null, 0);
    public static void PushBackChess(Chess chessOne) {
        if(chessOne == null) {
            return;
        }
        chessList.Add(chessOne);
        ResetAllChessPos();
    }

    public static bool RemoveChess(GameObject chessObj) {
        bool result = chessList.Remove(chessObj.GetComponent<Chess>());
        Destroy(chessObj);
        ResetAllChessPos();
        return result;
    }

    public static bool DoPreview(Chess chessOne) {
        previewChess = new KeyValuePair<GameObject, int>(chessOne.gameObject, chessList.IndexOf(chessOne));
        bool result = chessList.Remove(chessOne);
        if (result) {
            GlobalScope.MoveToLocal(chessOne.gameObject, previewPos, Quaternion.identity, 100f , true);
            previewChess.Key.GetComponent<Chess>().enabled = false;
            previewChess.Key.GetComponent<BoxCollider>().enabled = false;
            ResetAllChessPos();
        }
        return result;
    }

    public static void CancelPreview() {
        if (previewChess.Key == null) {
            Debug.Log("CancelPreview Nothing to do.");
            return;
        }
        chessList.Insert(previewChess.Value, previewChess.Key.GetComponent<Chess>());
        previewChess.Key.GetComponent<Chess>().enabled = true;
        previewChess.Key.GetComponent<BoxCollider>().enabled = true;
        previewChess = new KeyValuePair<GameObject, int>(null, 0);
        ResetAllChessPos();
    }

    public static void ResetAllChessPos() {
        float chessGap = ChessSelectorLength / (chessList.Count + 1);
        for (int i = 0; i < chessList.Count; i++) {
            chessList[i].chessPos = new Vector3(0, 0, - i * chessGap);
        }
    }
}