using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Managers;
using Gun;
using static UnityEditor.Profiling.RawFrameDataView;

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

    FMOD.Studio.EventInstance S_musicLoop;


    public static void PlayFmodEvent(string fmodEvent, Vector3 pos)
    {
        RuntimeManager.PlayOneShot("event:/" + fmodEvent, pos);
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

    public static void FireBulletSound(GunModule.BulletEffect bulletType, Vector3 position)
    {
        FMOD.Studio.EventInstance eventInstance = RuntimeManager.CreateInstance("event:/SFX/Player/Weapon_Shot");
        switch (bulletType)
        {
            case GunModule.BulletEffect.None:
                eventInstance.setParameterByName("Weapon_Type", 0, true);
                break;
            case GunModule.BulletEffect.Fire:
                eventInstance.setParameterByName("Weapon_Type", 1, true);
                break;
            case GunModule.BulletEffect.Ice:
                eventInstance.setParameterByName("Weapon_Type", 2, true);
                break;
            case GunModule.BulletEffect.Lightning:
                eventInstance.setParameterByName("Weapon_Type", 0, true);
                break;
            case GunModule.BulletEffect.Vampire:
                eventInstance.setParameterByName("Weapon_Type", 3, true);
                break;
        }
        eventInstance.start();
    }


    public static void StartMusicLoop()
    {
        audioManager.S_musicLoop.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        audioManager.S_musicLoop = StartFmodLoop("event:/Music/Main Music Timeline");
        TransitionToMainMenu();
    }

    public static void StopMusicLoop()
    {
        EndFmodLoop(audioManager.S_musicLoop);
    }

    public static void TransitionToMainMenu()
    {
        audioManager.S_musicLoop.setParameterByName("MainMusicTransition", 0);
    }
    public static void TransitionToBattleTheme()
    {
        audioManager.S_musicLoop.setParameterByName("MainMusicTransition", 1);
    }

    public static void SetBattleMusicLowIntensity()
    {
        audioManager.S_musicLoop.setParameterByName("BattleThemeIntensity", 0);
    }
    public static void SetBattleMusicHighIntensity()
    {
        audioManager.S_musicLoop.setParameterByName("BattleThemeIntensity", 1);
    }
}
