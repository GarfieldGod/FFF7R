using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSave : MonoBehaviour
{
    void Update()
    {
        App.Save.Flush();
    }
}
