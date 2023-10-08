using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
            pauseMenu.gameObject.SetActive(true);
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
            GameManager.SwitchToInGameActions();
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