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
            private set;
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (pauseMenu != null && pauseMenu != this)
            {
                Destroy(this);
            }
            else
            {
                pauseMenu = this;
            }
        }

        static public void PauseGame()
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.b_gamePaused = true;
            Time.timeScale = 0;
        }

        static public void UnpauseGame()
        {
            pauseMenu.gameObject.SetActive(false);
            pauseMenu.b_gamePaused = false;
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