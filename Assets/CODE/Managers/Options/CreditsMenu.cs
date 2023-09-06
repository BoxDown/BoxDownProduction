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
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
