using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
    public string chessName;
    public int level;
    public int cost;
    public Vector3 chessPos = new Vector3(0, 0, 0);
    public GameObject chessModel = null;
    public void InstantiateChessProperty(GlobalScope.ChessProperty property)
    {
        gameObject.name = property.Name;
        chessName = property.Name;
        level = property.Level;
        cost = property.Cost;

        foreach(Transform child in transform) {
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
                case "chessModel":
                    chessModel = child.gameObject;
                    break;
                default:break;
            }
        }
    }
    public void ObjectPosToChessPos(Vector3 position, Quaternion rotation, float moveSpeed){
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, moveSpeed * Time.deltaTime);
        transform.localRotation = rotation;
    }
    void MouseHover() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f) && hit.collider.gameObject == gameObject) {
            chessModel.transform.localPosition = Vector3.MoveTowards(chessModel.transform.localPosition, new Vector3(1, 0, 0), 30f * Time.deltaTime);
        } else {
            chessModel.transform.localPosition = Vector3.MoveTowards(chessModel.transform.localPosition, new Vector3(0, 0, 0), 30f * Time.deltaTime);
        }
    }
    void Update () {
        ObjectPosToChessPos(chessPos, Quaternion.identity, 100f);
        MouseHover();
    }
}