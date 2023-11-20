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
        [Rename("Time Between Animations")] public float f_waitTime = 1;
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
            resultsUI.C_stat4Name.text = "Time:";
            resultsUI.C_stat4Value.text = $"{((int)(GameManager.GetTimeTaken() / 60)).ToString("0")}m { (GameManager.GetTimeTaken() % 60).ToString("f2")}s";
        }

        public static void ActivateLose()
        {
            resultsUI.StartCoroutine(resultsUI.ResultsAppearOverTime(false));
        }
        public static void ActivateWin()
        {
            resultsUI.StartCoroutine(resultsUI.ResultsAppearOverTime(true));
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

        public static void ResetResults()
        {
            resultsUI.C_winResult.gameObject.SetActive(false);
            resultsUI.C_loseResult.gameObject.SetActive(false);
            resultsUI.C_statGroupTransform.gameObject.SetActive(false);
            resultsUI.C_statGroup2Transform.gameObject.SetActive(false);
            resultsUI.C_gunGroup.gameObject.SetActive(false);
            resultsUI.C_buttonGroup.gameObject.SetActive(false);

        }

        public IEnumerator ResultsAppearOverTime(bool win)
        {
            UpdateStats();
            InGameUI.DeactivateInGameUI();

            float gameOverStartTime = Time.time;
            if (win)
            {
                resultsUI.C_loseResult.gameObject.SetActive(false);
                resultsUI.C_winResult.gameObject.SetActive(true);
                while (Time.time - gameOverStartTime < f_waitTime)
                {
                    float percentage = (Time.time - gameOverStartTime) / f_waitTime;
                    float scale = C_scaleCurve.Evaluate(percentage);
                    resultsUI.C_winResult.transform.localScale = Vector3.one * scale;
                    yield return 0;
                }
            }
            else
            {
                resultsUI.C_winResult.gameObject.SetActive(false);
                resultsUI.C_loseResult.gameObject.SetActive(true);
                while (Time.time - gameOverStartTime < f_waitTime)
                {
                    float percentage = (Time.time - gameOverStartTime) / f_waitTime;
                    float scale = C_scaleCurve.Evaluate(percentage);
                    resultsUI.C_loseResult.transform.localScale = Vector3.one * scale;
                    yield return 0;
                }
            }

            float group1StartTime = Time.time;
            C_statGroupTransform.gameObject.SetActive(true);
            while (Time.time - group1StartTime < f_waitTime)
            {
                float percentage = (Time.time - group1StartTime) / f_waitTime;
                float scale = C_scaleCurve.Evaluate(percentage);
                C_stat1Name.transform.localScale = Vector3.one * scale;
                C_stat2Name.transform.localScale = Vector3.one * scale;
                yield return 0;
            }
            C_stat1Name.transform.localScale = Vector3.one;
            C_stat2Name.transform.localScale = Vector3.one;



            float group2StartTime = Time.time;
            C_statGroup2Transform.gameObject.SetActive(true);
            while (Time.time - group2StartTime < f_waitTime)
            {
                float percentage = (Time.time - group2StartTime) / f_waitTime;
                float scale = C_scaleCurve.Evaluate(percentage);
                C_stat3Name.transform.localScale = Vector3.one * scale;
                C_stat4Name.transform.localScale = Vector3.one * scale;
                yield return 0;
            }
            C_stat3Name.transform.localScale = Vector3.one;
            C_stat4Name.transform.localScale = Vector3.one;


            float group3StartTime = Time.time;
            C_gunGroup.gameObject.SetActive(true);
            while (Time.time - group3StartTime < f_waitTime)
            {
                float percentage = (Time.time - group3StartTime) / f_waitTime;
                float scale = C_scaleCurve.Evaluate(percentage);
                C_gunGroup.transform.localScale = Vector3.one * scale;
                yield return 0;
            }
            C_gunGroup.transform.localScale = Vector3.one;

            float group4StartTime = Time.time;
            C_buttonGroup.gameObject.SetActive(true);
            while (Time.time - group4StartTime < f_waitTime)
            {
                float percentage = (Time.time - group4StartTime) / f_waitTime;
                float scale = C_scaleCurve.Evaluate(percentage);
                C_buttonGroup.transform.localScale = Vector3.one * scale;
                yield return 0;
            }
            C_buttonGroup.transform.localScale = Vector3.one;

            GameManager.CurrentSelectionResultsMenu();
        }
    }
}