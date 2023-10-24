using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility;

namespace Managers
{
    public class ResultsUI : MonoBehaviour
    {

        [Rename("Lose Screen")] public Transform C_loseResult;
        [Rename("Lose Text")] public TextMeshProUGUI C_loseText;
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
            resultsUI.C_loseText.text = $"-You Died-";
            resultsUI.C_statsText.text =
                "Stats:\n" + 
                $"Spiders Killed: {GameManager.GetSpidersKilled()}\n" +
                $"Mites Killed: {GameManager.GetMitesKilled()}\n" +
                $"Slugs Killed: {GameManager.GetSlugsKilled()}\n" +
                $"Wasps Killed: {GameManager.GetWaspsKilled()}\n" +
                $"Triggers Swapped: {GameManager.GetTriggerSwaps()}\n" +
                $"Clips Swapped: {GameManager.GetClipSwaps()}\n" +
                $"Barrels Swapped: {GameManager.GetBarrelSwaps()}\n" +
                $"Rooms Cleared: {GameManager.GetRoomsCleared()}\n" +
                $"Amount of Explosions: {GameManager.GetExplosionCount()}\n" +
                $"Damage Taken: {GameManager.GetDamageTaken()}\n" +
                $"Health Regained: {GameManager.GetHealthRegained()}\n" +
                $"Times Dodged: {GameManager.GetDodgeCount()}\n" +
                $"Bullets Fired: {GameManager.GetBulletsFired()}\n" +
                $"Bullets Hit: {GameManager.GetBulletsHit()}\n" +
                $"Weapon Accuracy: {GameManager.GetHitRate().ToString("f1")}%\n" +
                $"Boxes Destoyed: {GameManager.GetEnvironmentDestroyed()}\n" +
                $"Play Time: {((int)(GameManager.GetTimeTaken()/ 60)).ToString("0")}m {(GameManager.GetTimeTaken() % 60).ToString("f2")}s\n" +
                $"Average Time Per Room: {((int)(GameManager.GetAverageTimePerRoom() / 60)).ToString("0")}m {(GameManager.GetAverageTimePerRoom() % 60).ToString("f2")}s\n";
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