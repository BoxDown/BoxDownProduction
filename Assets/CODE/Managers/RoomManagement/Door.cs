using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    private bool b_locked = true;
    Material C_doorMat;

    public enum RoomType
    {
        None,
        Trigger,
        Clip,
        Barrel,
        RandomModule,
        Count
    }

    private RoomType e_roomType = RoomType.None;


    // Start is called before the first frame update
    void Start()
    {
        b_locked = true;
        GetComponent<BoxCollider>().isTrigger = true;
        C_doorMat = new Material(Shader.Find("HDRP/Lit"));
        RandomiseRoomType();
        GetComponent<Renderer>().material = C_doorMat;
        UpdateDoorVisuals();
    }

    private void RandomiseRoomType()
    {
        //TO DO FIX SO THAT THERE IS NOT AN IMPOSSIBILITY THAT THERE ARE ROOMS WITH NO REWARDS
        e_roomType = (RoomType)Random.Range(1, (int)RoomType.Count);        
    }

    private void UpdateDoorVisuals()
    {
        switch (e_roomType)
        {
            case RoomType.None:
                C_doorMat.SetColor("_BaseColor", Color.white);
                break;
            case RoomType.Trigger:
                C_doorMat.SetColor("_BaseColor", Color.blue);
                break;
            case RoomType.Clip:
                C_doorMat.SetColor("_BaseColor", Color.green);
                break;
            case RoomType.Barrel:
                C_doorMat.SetColor("_BaseColor", Color.yellow);
                break;
            case RoomType.RandomModule:
                C_doorMat.SetColor("_BaseColor", Color.red);
                break;
        }
    }

    public void OnEnterDoor()
    {
        GameManager.gameManager.UpdateRewardType(e_roomType);
        GameManager.gameManager.MoveToNextRoom();
    }

    public void Unlock()
    {
        b_locked = false;
    }

    public bool IsLocked()
    {
        return b_locked;
    }

}
