using System;
using UnityEngine;
public class Log {
    public static void test (string info) {
    #if UNITY_ENGINE
        Debug.Log(info);
    #else
        Console.WriteLine(info);
    #endif
    }
}