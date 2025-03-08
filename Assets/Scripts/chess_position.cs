using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class chess_position : MonoBehaviour
{
    public GlobalScope.ChessPosStatus posStatus = GlobalScope.ChessPosStatus.EMPTY;
    public int level = 0;
    GameObject levelOne;
    GameObject levelTwo;
    GameObject levelThree;
    // Start is called before the first frame update
    void Start()
    {
        levelOne = transform.Find("levelOne").gameObject;
        levelTwo = transform.Find("levelTwo").gameObject;
        levelThree = transform.Find("levelThree").gameObject;
        ShowPosLevel();
    }

    // Update is called once per frame
    void Update()
    {
        ShowPosLevel();
    }
    public void UpdateStatus() {

    }
    public void ShowPosLevel() {
        switch (posStatus) {
            case GlobalScope.ChessPosStatus.LEVEL_ONE_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_ONE_ENEMY:
                levelOne.SetActive(true);
                levelTwo.SetActive(false);
                levelThree.SetActive(false);
                break;
            case GlobalScope.ChessPosStatus.LEVEL_TWO_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_TWO_ENEMY:
                levelOne.SetActive(false);
                levelTwo.SetActive(true);
                levelThree.SetActive(false);
                break;
            case GlobalScope.ChessPosStatus.LEVEL_THREE_FRIEND:
            case GlobalScope.ChessPosStatus.LEVEL_THREE_ENEMY:
                levelOne.SetActive(false);
                levelTwo.SetActive(false);
                levelThree.SetActive(true);
                break;
            default: 
                levelOne.SetActive(false);
                levelTwo.SetActive(false);
                levelThree.SetActive(false);
                break;
        }
        foreach (Transform child in transform)
        {
            foreach (Transform childChild in child)
            {
                Renderer renderer = childChild.gameObject.GetComponent<Renderer>();
                if (posStatus < GlobalScope.ChessPosStatus.EMPTY) {
                    renderer.material.color = Color.green;
                } else {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
}
