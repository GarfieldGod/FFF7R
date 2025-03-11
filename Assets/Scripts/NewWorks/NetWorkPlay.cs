using Mirror;
using UnityEngine;

public class NetworkPlay : NetworkBehaviour
{
    [TargetRpc]
    public void TargetSendContent(NetworkConnection target, string content)
    {
        Debug.Log($"Received content: {content}");
    }
}