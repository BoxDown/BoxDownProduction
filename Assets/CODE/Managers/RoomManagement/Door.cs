using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using Utility;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    private bool b_locked = true;
    [Rename("Cone Transform"), SerializeField] private Transform C_coneTransform;
    [Rename("Projection Transform"), SerializeField] private Transform C_projectionTransform;
    [Rename("Trigger Cone Material"), SerializeField] private Material C_triggerConeMat;
    [Rename("Trigger Projection Material"), SerializeField] private Material C_triggerProjectionMat;
    [Rename("Clip Cone Material"), SerializeField] private Material C_clipConeMat;
    [Rename("Clip Projection Material"), SerializeField] private Material C_clipProjectionMat;
    [Rename("Barrel Cone Material"), SerializeField] private Material C_barrelConeMat;
    [Rename("Barrel Projection Material"), SerializeField] private Material C_barrelProjectionMat;
    [Rename("Random Cone Material"), SerializeField] private Material C_randomConeMat;
    [Rename("Random Projection Material"), SerializeField] private Material C_randomProjectionMat;
    private List<Door> lC_doors = new List<Door>();

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
        GetAllDoors();
        RandomiseRoomType();
        UpdateDoorVisuals();
        C_coneTransform.gameObject.SetActive(false);
        C_projectionTransform.gameObject.SetActive(false);
    }

    public void GetAllDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].transform == transform)
            {
                continue;
            }
            lC_doors.Add(doors[i]);
        }
    }

    private void RandomiseRoomType()
    {
        bool doorDifferentToOthers = false;
        while (!doorDifferentToOthers)
        {
            e_roomType = (RoomType)Random.Range(1, (int)RoomType.Count);
            foreach (Door d in lC_doors)
            {
                if (d.e_roomType == e_roomType)
                {
                    doorDifferentToOthers = false;
                    break;
                }
                doorDifferentToOthers = true;
            }
        }
    }

    private void UpdateDoorVisuals()
    {
        //do new stuff
        switch (e_roomType)
        {
            case RoomType.None:
                break;
            case RoomType.Trigger:
                C_coneTransform.GetComponent<MeshRenderer>().material = C_triggerConeMat;
                C_projectionTransform.GetComponent<MeshRenderer>().material = C_triggerProjectionMat;
                break;
            case RoomType.Clip:
                C_coneTransform.GetComponent<MeshRenderer>().material = C_clipConeMat;
                C_projectionTransform.GetComponent<MeshRenderer>().material = C_clipProjectionMat;
                break;
            case RoomType.Barrel:
                C_coneTransform.GetComponent<MeshRenderer>().material = C_barrelConeMat;
                C_projectionTransform.GetComponent<MeshRenderer>().material = C_barrelProjectionMat;
                break;
            case RoomType.RandomModule:
                C_coneTransform.GetComponent<MeshRenderer>().material = C_randomConeMat;
                C_projectionTransform.GetComponent<MeshRenderer>().material = C_randomProjectionMat;
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

        C_coneTransform.gameObject.SetActive(true);
        C_projectionTransform.gameObject.SetActive(true);
    }

    public bool IsLocked()
    {
        return b_locked;
    }

}
