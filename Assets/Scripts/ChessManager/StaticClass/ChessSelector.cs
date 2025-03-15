using System.Collections.Generic;
using UnityEngine;

public class ChessSelector : MonoBehaviour
{
    public static List<Chess> chessList_ = new List<Chess>{};
    private static float ChessSelectorLength_ = 50;
    private static Vector3 previewPos_ = new Vector3(22, -5, 3);
    private static KeyValuePair<GameObject, int> previewChess_ = new KeyValuePair<GameObject, int>(null, 0);
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

    public static bool DoPreview(Chess chessOne) {
        previewChess_ = new KeyValuePair<GameObject, int>(chessOne.gameObject, chessList_.IndexOf(chessOne));
        bool result = chessList_.Remove(chessOne);
        if (result) {
            GlobalScope.MoveToLocal(chessOne.gameObject, previewPos_, Quaternion.identity, 100f , true);
            previewChess_.Key.GetComponent<Chess>().enabled = false;
            previewChess_.Key.GetComponent<BoxCollider>().enabled = false;
            ResetAllChessPos();
        }
        return result;
    }

    public static void CancelPreview() {
        if (previewChess_.Key == null) {
            Debug.Log("CancelPreview Nothing to do.");
            return;
        }
        chessList_.Insert(previewChess_.Value, previewChess_.Key.GetComponent<Chess>());
        previewChess_.Key.GetComponent<Chess>().enabled = true;
        previewChess_.Key.GetComponent<BoxCollider>().enabled = true;
        previewChess_ = new KeyValuePair<GameObject, int>(null, 0);
        ResetAllChessPos();
    }

    public static void ResetAllChessPos() {
        float chessGap = ChessSelectorLength_ / (chessList_.Count + 1);
        for (int i = 0; i < chessList_.Count; i++) {
            chessList_[i].chessPos = new Vector3(0, - i * 0.1f, - i * chessGap);
        }
    }
}