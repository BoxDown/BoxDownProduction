using System.Collections;
using TMPro;
using UnityEditor.Build;
using UnityEngine;
using Utility;

namespace Managers
{
    public class ResultsUI : MonoBehaviour
    {

        [Rename("Lose Screen")] public Transform C_loseResult;
        [Rename("Win Screen")] public Transform C_winResult;


        [Header("Stat Group 1")]
        [Rename("Stat Group 1 Transform")] public Transform C_statGroupTransform;
        [Rename("Stat 1 Name")] public TextMeshProUGUI C_stat1Name;
        [Rename("Stat 1 Value")] public TextMeshProUGUI C_stat1Value;
        [Rename("Stat 2 Name")] public TextMeshProUGUI C_stat2Name;
        [Rename("Stat 2 Value")] public TextMeshProUGUI C_stat2Value;

        [Header("Stat Group 2")]
        [Rename("Stat Group 2 Transform")] public Transform C_statGroup2Transform;
        [Rename("Stat 3 Name")] public TextMeshProUGUI C_stat3Name;
        [Rename("Stat 3 Value")] public TextMeshProUGUI C_stat3Value;
        [Rename("Stat 4 Name")] public TextMeshProUGUI C_stat4Name;
        [Rename("Stat 4 Value")] public TextMeshProUGUI C_stat4Value;

        [Header("Groups")]
        [Rename("Gun Group")] public Transform C_gunGroup;
        [Rename("Button Group")] public Transform C_buttonGroup;

        [Header("Animation Variables")]
        [Rename("Time Between Animations")] public float f_waitTime;
        [Rename("Text Size Animation Curve")] public AnimationCurve C_scaleCurve;



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

        public static void ActivateResults()
        {
            resultsUI.gameObject.SetActive(true);
            ActivateLose();
        }
        public static void DeactivateResults()
        {
            resultsUI.gameObject.SetActive(false);
            DeactivateLose();
        }

        public static void UpdateStats()
        {
            resultsUI.C_stat1Name.text = "Rooms Cleared:";
            resultsUI.C_stat1Value.text = GameManager.GetRoomsCleared().ToString();
            resultsUI.C_stat2Name.text = "Enemies Killed:";
            resultsUI.C_stat2Value.text = (GameManager.GetSpidersKilled() + GameManager.GetMitesKilled() + GameManager.GetSlugsKilled() + GameManager.GetWaspsKilled()).ToString();
            resultsUI.C_stat3Name.text = "Bullets Fired:";
            resultsUI.C_stat3Value.text = GameManager.GetBulletsFired().ToString();
            resultsUI.C_stat4Name.text = "This God Damn Stat";
            resultsUI.C_stat4Value.text = $"{((int)(GameManager.GetTimeTaken() / 60)).ToString("0")}m { (GameManager.GetTimeTaken() % 60).ToString("f2")}s";
        }

        public static void ActivateLose()
        {
            InGameUI.DeactivateInGameUI();
            resultsUI.C_winResult.gameObject.SetActive(false);
            resultsUI.C_loseResult.gameObject.SetActive(true);
            UpdateStats();
            GameManager.CurrentSelectionResultsMenu();
        }
        public static void ActivateWin()
        {
            InGameUI.DeactivateInGameUI();
            resultsUI.C_loseResult.gameObject.SetActive(false);
            resultsUI.C_winResult.gameObject.SetActive(true);
            UpdateStats();
            GameManager.CurrentSelectionResultsMenu();
        }
        public static void DeactivateLose()
        {
            resultsUI.gameObject.SetActive(false);
        }
        public static void RestartGame()
        {
            GameManager.RestartGame();
        }
        public static void ReturnToMain()
        {
            GameManager.BackToMainMenu();
        }

        public IEnumerator ResultsAppearOverTime(bool win)
        {
            yield return 0;
        }
    }
}