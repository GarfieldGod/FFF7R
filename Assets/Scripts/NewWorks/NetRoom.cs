using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetRoom : NetworkRoomManager
{
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
}
