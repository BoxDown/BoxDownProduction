using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Managers
{
    public class ResultsUI : MonoBehaviour
    {

        [Rename("Win Screen")] public Transform C_winResult;
        [Rename("Lose Screen")] public Transform C_loseResult;

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

        public static void ActivateWin()
        {
            resultsUI.C_winResult.gameObject.SetActive(true);
        }

        public static void ActivateLose()
        {
            resultsUI.C_loseResult.gameObject.SetActive(true);
        }

        public static void DeactivateWin()
        {
            resultsUI.C_winResult.gameObject.SetActive(false);
        }

        public static void DeactivateLose()
        {
            resultsUI.C_loseResult.gameObject.SetActive(false);
        }
        public static void RestartGame()
        {
            GameManager.RestartGame();
        }
        public static void ReturnToMain()
        {
            GameManager.BackToMainMenu();
        }
    }
}