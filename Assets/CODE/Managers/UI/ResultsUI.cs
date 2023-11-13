using TMPro;
using UnityEngine;
using Utility;

namespace Managers
{
    public class ResultsUI : MonoBehaviour
    {

        [Rename("Lose Screen")] public Transform C_loseResult;
        [Rename("Win Screen")] public Transform C_winResult;
        [Rename("Stats Text")] public TextMeshProUGUI C_statsText;



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

        public static void ActivateLose()
        {
            InGameUI.DeactivateInGameUI();
            resultsUI.C_loseResult.gameObject.SetActive(true);
            GameManager.CurrentSelectionResultsMenu();
            resultsUI.C_statsText.text =
                "Stats:\n" +
                $"Rooms Cleared: \t \t {GameManager.GetRoomsCleared()}\n \v" +
                $"Enemies Killed: \t \t {GameManager.GetSpidersKilled() + GameManager.GetMitesKilled() + GameManager.GetSlugsKilled() + GameManager.GetWaspsKilled()}\n \v" +
                $"Modules Swapped: \t  {GameManager.GetTriggerSwaps() + GameManager.GetClipSwaps() + GameManager.GetBarrelSwaps()}\n \v" +
                $"Play Time: \t \t {((int)(GameManager.GetTimeTaken() / 60)).ToString("0")}m {(GameManager.GetTimeTaken() % 60).ToString("f2")}s\n \v" +
                $"Bullets Fired: \t \t {GameManager.GetBulletsFired()}\n \v" +
                $"Weapon Accuracy: \t  {GameManager.GetHitRate().ToString("f1")}%\n";
        }
        public static void ActivateWin()
        {
            InGameUI.DeactivateInGameUI();
            resultsUI.C_winResult.gameObject.SetActive(true);
            GameManager.CurrentSelectionResultsMenu();
            resultsUI.C_statsText.text =
                "Stats:\n" +
                $"Rooms Cleared: \t \t {GameManager.GetRoomsCleared()}\n \v" +
                $"Enemies Killed: \t \t {GameManager.GetSpidersKilled() + GameManager.GetMitesKilled() + GameManager.GetSlugsKilled() + GameManager.GetWaspsKilled()}\n \v" +
                $"Modules Swapped: \t  {GameManager.GetTriggerSwaps() + GameManager.GetClipSwaps() + GameManager.GetBarrelSwaps()}\n \v" +
                $"Play Time: \t \t {((int)(GameManager.GetTimeTaken() / 60)).ToString("0")}m {(GameManager.GetTimeTaken() % 60).ToString("f2")}s\n \v" +
                $"Bullets Fired: \t \t {GameManager.GetBulletsFired()}\n \v" +
                $"Weapon Accuracy: \t  {GameManager.GetHitRate().ToString("f1")}%\n";
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