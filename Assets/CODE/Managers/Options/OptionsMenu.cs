using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class OptionsMenu : MonoBehaviour
    {
        static public OptionsMenu optionsMenu
        {
            get;
            private set;
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (optionsMenu != null && optionsMenu != this)
            {
                Destroy(this);
            }
            else
            {
                optionsMenu = this;
            }
        }
        public static void Activate()
        {
            optionsMenu.gameObject.SetActive(true);
        }
        public static void Deactivate()
        {
            optionsMenu.gameObject.SetActive(false);
        }

        // TO DO ACTUALLY FILL IN THESE STUBS
        public void SetVolume()
        {
            
        }
        public void SetMusicVolume()
        {

        }
        public void SetSFXVolume()
        {

        }
        public void SetResolution()
        {

        }
        public void SetQuality()
        {
            
        }
        public void SetFullScreen()
        {

        }
    }
}
