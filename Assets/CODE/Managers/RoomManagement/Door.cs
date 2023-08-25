using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door : MonoBehaviour
{
    private bool b_locked = true;

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
    }

    private void RandomiseRoomType()
    {
        //TO DO FIX SO THAT THERE IS NOT AN IMPOSSIBILITY THAT THERE ARE ROOMS WITH NO REWARDS
        e_roomType = (RoomType)Random.Range(1, (int)RoomType.Count);
    }

    private void OnEnterDoor()
    {
        GameManager.gameManager.UpdateRewardType(e_roomType);
        GameManager.gameManager.MoveToNextRoom();
    }

    public void Unlock()
    {
        b_locked = false;
    }

}
