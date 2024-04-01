using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SplashScreenController : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(Load_Game),0.5f);
    }

    private void Load_Game()
    {
        SceneController.Instance.GoTo_GamePlayScene_At_SplashOff();
    }

    public void LoadNextScreen()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}