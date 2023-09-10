using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility;

namespace Managers
{
    public class ResultsUI : MonoBehaviour
    {

        [Rename("Win Screen")] public Transform C_winResult;
        [Rename("Lose Screen")] public Transform C_loseResult;
        [Rename("Lose Text")] public TextMeshProUGUI C_loseText;


        public static ResultsUI resultsUI
        {
            get;
            private set;
        }
        void Awake()
        {
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
            resultsUI.C_loseText.text = $"You Made It Through {GameManager.gameManager.i_currentRoom - 1} rooms!";
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