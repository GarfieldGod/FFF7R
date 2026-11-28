using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSave : MonoBehaviour
{

    void Start()
    {
        foreach (var code in App.Cards.AllCodes)
        {
            GameAssets.Sprites.Request(code, null);
        }
    }
    void Update()
    {
        App.Save.Flush();
    }
}
