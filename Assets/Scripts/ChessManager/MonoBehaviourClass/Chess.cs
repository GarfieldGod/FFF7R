using UnityEngine;

public class Chess : MonoBehaviour
{
    public string cardCode;
    public string chessName;
    public int level;
    public int cost;
    public Vector3 chessPos = new Vector3(0, 0, 0);
    public GameObject chessModel = null;
    public void InstantiateChessProperty(ChessProperty property)
    {
        gameObject.name = property.CardCode;
        cardCode = property.CardCode;
        chessName = property.Name;
        level = property.Level;
        cost = property.Cost;
    }
    private void ObjectPosToChessPos(Vector3 position, Quaternion rotation, float moveSpeed){
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, moveSpeed * Time.deltaTime);
        transform.localRotation = rotation;
    }
    private void MouseHover() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f) && hit.collider.gameObject == gameObject) {
            chessModel.transform.localPosition = Vector3.MoveTowards(chessModel.transform.localPosition, new Vector3(1, 1, 0), 30f * Time.deltaTime);
        } else {
            chessModel.transform.localPosition = Vector3.MoveTowards(chessModel.transform.localPosition, new Vector3(0, 0, 0), 30f * Time.deltaTime);
        }
    }
    void Update () {
        ObjectPosToChessPos(chessPos, Quaternion.identity, 100f);
        if (chessModel != null) {
            MouseHover();
        }
    }
}