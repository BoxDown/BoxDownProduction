using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{

    public class WeaponsSwapUI : MonoBehaviour
    {
        static public WeaponsSwapUI swapUI
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (swapUI != null && swapUI != this)
            {
                Destroy(this);
            }
            else
            {
                swapUI = this;
            }
        }
        public static void Activate()
        {
            swapUI.gameObject.SetActive(true);
        }
        public static void Deactivate()
        {
            swapUI.gameObject.SetActive(false);
        }
    }

}