using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeUI : MonoBehaviour
{
    [SerializeField] Canvas uiCanvas;

    GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }
    private void OnEnable()
    {
        if (gameManager.currentScene == "Game")
        {
            uiCanvas.worldCamera = GameObject.Find("GameSceneCam").GetComponent<Camera>();
            uiCanvas.planeDistance = 6;
        }
            

    }

}
