using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ChessManager : MonoBehaviour
{

}

public class Chess
{
    public GameObject chessBody;
    public string name;
    public int level;
    public int cost;
    // public Vector3 chessPos = new Vector3(0, 0, 0);
    public List<List<int>> posEffects;
    public List<List<int>> cardeffects;
    public HashSet<string> specialEffects;
    public Chess(GlobalScope.ChessProperty property, GameObject chessBody)
    {
        this.name = property.Name;
        this.level = property.Level;
        this.cost = property.Cost;
        this.posEffects = property.PosEffects ?? new List<List<int>>();
        this.cardeffects = property.CardEffects ?? new List<List<int>>();
        this.specialEffects = property.SpecialEffects ?? new HashSet<string>();

        this.chessBody = chessBody;
        chessBody.name = name;
        foreach(Transform child in chessBody.transform) {
            if(child.name == "") {
                child.gameObject.GetComponent<TextMesh>().text = "";
            }
            switch (child.name) {
                case "name":
                    child.gameObject.GetComponent<TextMesh>().text = name;
                    break;
                case "level":
                    child.gameObject.GetComponent<TextMesh>().text = "Level:" + level.ToString();
                    break;
                case "cost":
                    child.gameObject.GetComponent<TextMesh>().text = "Cost:" + cost.ToString();
                    break;
                default:break;
            }
        }
        ObjectPosToChessPos(Vector3.zero, Quaternion.identity);
    }
    public void ObjectPosToChessPos(Vector3 position, Quaternion rotation){
        chessBody.transform.localPosition = position;
        chessBody.transform.localRotation = rotation;
    }
}

public class ChessSelector
{
    static Vector3 ChessSelectorPos = new Vector3(0, 0, 0);
    static float ChessSelectorLength = 60;
    public static List<Chess> chessList = new List<Chess>{};
    public static KeyValuePair<Chess, int> previewChess = new KeyValuePair<Chess, int>(null, 0);

    public static void PushBackChess(Chess chessOne) {
        if(chessOne == null) {
            return;
        }
        chessList.Add(chessOne);
        ResetAllChessPos();
    }

    public static void ResetAllChessPos() {
        float chessGap = ChessSelectorLength / (chessList.Count + 1);
        for (int i = 0; i < chessList.Count; i++) {
            // chessList[i].chessPos = new Vector3(ChessSelectorPos.x + (i + 1) * chessGap, ChessSelectorPos.y, ChessSelectorPos.z);
            chessList[i].ObjectPosToChessPos(new Vector3(ChessSelectorPos.x, ChessSelectorPos.y, ChessSelectorPos.z - i * chessGap), Quaternion.identity);
        }
    }

    public static bool RemoveChess(Chess chessOne) {
        bool result = chessList.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public static bool DoPreview(Chess chessOne) {
        Debug.Log("------------------------------DoPreview");
        previewChess = new KeyValuePair<Chess, int>(chessOne, chessList.IndexOf(chessOne));
        bool result = chessList.Remove(chessOne);
        ResetAllChessPos();
        return result;
    }

    public static void CancelPreview() {
        Debug.Log("------------------------------CancelPreview");
        if (previewChess.Key == null) {
            Debug.Log("CancelPreview Nothing to do.");
            return;
        }
        chessList.Insert(previewChess.Value, previewChess.Key);
        previewChess = new KeyValuePair<Chess, int>(null, 0);
        ResetAllChessPos();
    }
}