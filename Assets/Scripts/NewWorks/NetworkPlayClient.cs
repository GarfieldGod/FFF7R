// using Mirror;
// using UnityEngine;
// using System;

// public struct NetWorkGameInfo : NetworkMessage {
//     public string GameName;
// }
// public class NetworkPlayClient : MonoBehaviour
// {
//     void Start()
//     {
//         NetworkClient.RegisterHandler<NetWorkGameInfo>(OnCustomStringMessageReceived);
//     }

//     private void OnCustomStringMessageReceived(NetWorkGameInfo message)
//     {
//         Debug.Log("Received message: " + message.GameName);

//         // // 根据是否是本地玩家或其他条件处理消息
//         // if (isLocalPlayer)
//         // {
//         //     HandleMessageAsLocalPlayer(message.message);
//         // }
//         // else
//         // {
//         //     HandleMessageAsRemotePlayer(message.message);
//         // }
//     }

//     // private void OnReceiveMessage(NetworkConnection conn, NetworkReader reader)
//     // {
//     //     // 反序列化消息
//     //     string message = reader.ReadString();

//     //     // 根据客户端的特定逻辑处理消息
//     //     if (clientHandlers.ContainsKey(conn.connectionId))
//     //     {
//     //         clientHandlers[conn.connectionId](message);
//     //     }
//     //     else
//     //     {
//     //         Debug.LogError($"No handler found for connection ID: {conn.connectionId}");
//     //     }
//     // }
// }
// public static class NetWorkExtensions {
    
//     public static void SerializeNetWorkGameInfo(this NetworkWriter writer, NetWorkGameInfo info) {

//     }
//     public static void DeserializeNetWorkGameInfo(this NetworkReader reader) {

//     }
// }