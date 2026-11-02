using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFF7R;

public class GameCommandRoot : MonoBehaviour
{
    [SerializeField] GameBootstrap bootstrap;

    IGameCommand command;

    void Awake()
    {
        if (bootstrap != null) bootstrap.SessionCreated += Bind;
    }

    void Bind(IGameCommand s)
    {
        command = s;
    }
}
