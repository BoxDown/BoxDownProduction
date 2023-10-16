using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager audioManager
    {
        get;
        private set;
    }
    public void Awake()
    {
        if (audioManager != null && audioManager != this)
        {
            Destroy(this);
        }
        else
        {
            audioManager = this;
        }
    }

    FMOD.Studio.EventInstance S_mainMenuLoop;
    FMOD.Studio.EventInstance S_battleLoop;


    public static void PlayFmodEvent(string fmodEvent, Vector3 pos)
    {
        RuntimeManager.PlayOneShot("event:/" + fmodEvent, pos);
        Debug.Log($"Played {fmodEvent}");
    }    

    public static FMOD.Studio.EventInstance StartFmodLoop(string fmodEvent)
    {
        FMOD.Studio.EventInstance eventInstance = RuntimeManager.CreateInstance(fmodEvent);
        eventInstance.start();
        return eventInstance;
    }
    public static void EndFmodLoop(FMOD.Studio.EventInstance fmodEvent)
    {
        fmodEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public static void StartMainMenuLoop()
    {
        audioManager.S_mainMenuLoop = StartFmodLoop("Music/Main Music Timeline");
    }

    public static void StopMainMenuLoop()
    {
        EndFmodLoop(audioManager.S_mainMenuLoop);
    }
    public static void StartBattleLoop()
    {
        audioManager.S_battleLoop = StartFmodLoop("Music/Main Music Timeline");
    }

    public static void StopBattleLoop()
    {
        EndFmodLoop(audioManager.S_battleLoop);
    }

    public static void BattleMusicCalm()
    {
        //audioManager.S_battleLoop.setParameterByName("Thing", 0);
    }
    public static void BattleMusicExciting()
    {
        //audioManager.S_battleLoop.setParameterByName("Thing", 1);
    }
}
