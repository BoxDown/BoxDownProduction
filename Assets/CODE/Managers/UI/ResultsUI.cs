using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class ResultsUI : MonoBehaviour
{

    Transform C_winResult;
    Transform C_loseResult;

    public static ResultsUI resultsUI
    {
        get;
        private set;
    }
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (resultsUI != null && resultsUI != this)
        {
            Destroy(this);
        }
        else
        {
            resultsUI = this;
        }
    }

    public void ActivateWin()
    {
        C_winResult.gameObject.SetActive(true);
    }

    public void ActivateLose()
    {
        C_loseResult.gameObject.SetActive(true);
    }

    public void DeactivateWin()
    {
        C_winResult.gameObject.SetActive(false);
    }

    public void DeactivateLose()
    {
        C_loseResult.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        GameManager.RestartGame();
    }
    public void ReturnToMain()
    {
        GameManager.BackToMainMenu();
    }
}
