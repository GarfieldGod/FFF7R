using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetRoom : NetworkRoomManager
{
    
    private int playerCount = 0;
    public static NetRoom instance;
    public override void Awake()
    {
        base.Awake();
        if(instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        playerCount++;
        switch (playerCount) {
            case 1:
                Debug.Log("当前共有1位玩家。");
                break;
            case 2:
                Debug.Log("当前共有2位玩家。");
                break;
            default:break;
        }
        if(playerCount > 2) {
            Debug.Log("The room is full.");
            conn.Disconnect();
        }
        Debug.Log("Player Entered: " + conn.connectionId.ToString());
        // SendCustomMessageToAll("Welcome to the game!");
    }
    public override void OnServerSceneChanged(string sceneName) {
        if(sceneName == GameplayScene) {
            Debug.Log("gameStart");
        }
    }
    // public static void SendCustomMessageToAll(string message)
    // {
    //     NetWorkGameInfo customMessage = new NetWorkGameInfo{
    //         GameName = message
    //     };

    //     foreach (var conn in NetworkServer.connections.Values)
    //     {
    //         if (conn != null && conn.isReady)
    //         {
    //             conn.Send(customMessage);
    //         }
    //     }
    // }
}
