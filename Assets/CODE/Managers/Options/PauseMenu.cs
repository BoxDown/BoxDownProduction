using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utility;

namespace Managers
{
    public class PauseMenu : MonoBehaviour
    {
        static public PauseMenu pauseMenu
        {
            get;
            private set;
        }
        public bool b_gamePaused
        {
            get;
            set;
        }

        [Rename("Trigger Module Card"), SerializeField] private GunModuleCard C_triggerCard;
        [Rename("Clip Module Card"), SerializeField] private GunModuleCard C_clipCard;
        [Rename("Barrel Module Card"), SerializeField] private GunModuleCard C_barrelCard;

        private void Awake()
        {
            if (pauseMenu != null && pauseMenu != this)
            {
                Destroy(this);
            }
            else
            {
                pauseMenu = this;
            }
        }

        public static void ActivatePause()
        {
            GameManager.gameManager.SetCulling(false);
            GameManager.gameManager.C_gunModuleUI.PlayPauseUI();
            pauseMenu.C_triggerCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[0], false);
            pauseMenu.C_clipCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[1], false);
            pauseMenu.C_barrelCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[2], false);

            pauseMenu.gameObject.SetActive(true);
            GameManager.CurrentSelectionPauseMenu();
        }
        public static void DeactivatePause()
        {
            pauseMenu.gameObject.SetActive(false);
        }

        static public void PauseGame()
        {
            ActivatePause();
            pauseMenu.b_gamePaused = true;
            GameManager.SwitchToUIActions();
            Time.timeScale = 0;
        }

        static public void UnpauseGame()
        {
            DeactivatePause();
            pauseMenu.b_gamePaused = false;
            if (!WeaponsSwapUI.swapUI.gameObject.activeInHierarchy)
            {
                GameManager.SwitchToInGameActions();
                GameManager.gameManager.SetCulling(true);
            }
            else
            {
                GameManager.CurrentSelectionSwapMenu();
            }
            Time.timeScale = 1;
        }


        static public void OpenOptions()
        {
            OptionsMenu.Activate();
        }
        static public void CloseOptions()
        {
            OptionsMenu.Deactivate();
        }
    }
}