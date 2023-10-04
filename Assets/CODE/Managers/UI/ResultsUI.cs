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

        public static void ActivateLose()
        {
            resultsUI.C_loseResult.gameObject.SetActive(true);
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
                $"Weapon Accuracy: {GameManager.GetHitRate()}\n" +
                $"Boxes Destoyed: {GameManager.GetEnvironmentDestroyed()}\n" +
                $"Play Time: {GameManager.GetTimeTaken()}\n" +
                $"Average Time Per Room: {GameManager.GetAverageTimePerRoom()}\n";
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