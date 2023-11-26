using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Managers;
using Gun;

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
        switch (bulletType)
        {
            case GunModule.BulletEffect.None:
                PlayFmodEvent("SFX/StandardShot", position);
                break;
            case GunModule.BulletEffect.Fire:
                PlayFmodEvent("SFX/FireShot", position);
                break;
            case GunModule.BulletEffect.Ice:
                PlayFmodEvent("SFX/IceShot", position);
                break;
            case GunModule.BulletEffect.Lightning:
                PlayFmodEvent("SFX/ElectricShot", position);
                break;
            case GunModule.BulletEffect.Vampire:
                PlayFmodEvent("SFX/VampireShot", position);
                break;
        }
    }

    public static void SetMusicVolume(float vol)
    {
        RuntimeManager.GetBus("bus:/Music").setVolume(vol);
    }
    public static void SetSoundVolume(float vol)
    {
        RuntimeManager.GetBus("bus:/SFX").setVolume(vol);
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
