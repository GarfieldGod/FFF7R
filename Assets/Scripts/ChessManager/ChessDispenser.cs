using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessDispenser : MonoBehaviour
{
    public GameObject chessSelectorPos_;
    public GameObject chessPrefab_;
    public GameObject chessModelPrefab_;
    public static List<string> chessPool = new List<string>{};
    public static string DispenseChess() {
        if (chessPool.Count == 0) {
            return null;
        }
        System.Random random = new System.Random();
        int randomIndex = random.Next(chessPool.Count);
        string chess = chessPool[randomIndex];
        chessPool.RemoveAt(randomIndex);
        return chess;
    }

    public void ReDispense(List<int> index) {
        List<Chess> popChessList = new List<Chess>{};
        foreach(int i in index) {
            if(i < 0 || i >= ChessSelector.chessList.Count) {
                Debug.Log("ReDispense: Invaild chessIndex.");
                continue;
            }
            popChessList.Add(ChessSelector.chessList[i]);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.chessList.Remove(chess);
        }
        foreach(Chess chess in popChessList) {
            ChessSelector.PushBackChess(InstantiateChess(DispenseChess()).GetComponent<Chess>());
        }
        foreach(Chess chess in popChessList) {
            chessPool.Add(chess.cardCode);
        }
    }

    public GameObject InstantiateChess(string chessName)
    {
        if (chessName == null) {
            return null;
        }
        ChessProperty instantiateData = GlobalScope.GetChessProperty(chessName);
        GameObject instantiateBody = Instantiate(chessPrefab_, chessSelectorPos_.transform);
        GameObject instantiateModel = InstantiateChessModel(chessModelPrefab_, instantiateBody, instantiateData);
        if (instantiateData != null) {
            Chess chess = instantiateBody.GetComponent<Chess>();
            chess.InstantiateChessProperty(instantiateData);
            chess.chessModel = instantiateModel;
            return instantiateBody;
        }
        return null;
    }
    public static GameObject InstantiateChessModel(GameObject chessModelPrefab, GameObject father, ChessProperty instantiateData)
    {
        if (father == null) {
            return null;
        }
        GameObject instantiateModel = Instantiate(chessModelPrefab, father.transform);
        instantiateModel.transform.localPosition = Vector3.zero;
        instantiateModel.transform.localRotation = Quaternion.Euler(-90, -90, 0);
        if (instantiateData != null) {
            foreach(Transform child in instantiateModel.transform) {
                switch (child.name) {
                    case "name":
                        child.gameObject.GetComponent<TextMesh>().text = instantiateData.Name;
                        break;
                    case "level":
                        child.gameObject.GetComponent<TextMesh>().text = instantiateData.Level.ToString();
                        break;
                    case "cost":
                        string costNum = "";
                        for(int i = 0; i<instantiateData.Cost ;i++) {
                            costNum+="●";
                        }
                        child.gameObject.GetComponent<TextMesh>().text = costNum;
                        break;
                    default:break;
                }
            }
            return instantiateModel;
        }
        return null;
    }


    public int GetChessNumInChessSelector() {
        int result = 0;
        foreach(Transform child in chessSelectorPos_.transform) {
            result++;
        }
        return result;
    }
}
