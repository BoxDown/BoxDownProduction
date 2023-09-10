using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    public static CreditsMenu creditsUI
    {
        get;
        private set;
    }
    public void Awake()
    {
        if (creditsUI != null && creditsUI != this)
        {
            Destroy(this);
        }
        else
        {
            creditsUI = this;
        }
    }
    static public void Activate()
    {
        creditsUI.gameObject.SetActive(true);
    }
    static public void Deactivate()
    {
        creditsUI.gameObject.SetActive(false);
    }
}
