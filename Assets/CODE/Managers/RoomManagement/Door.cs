using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using Utility;
using UnityEngine.UIElements;

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
    [Rename("Boss Cone Material"), SerializeField] private Material C_bossConeMat;
    [Rename("Boss Projection Material"), SerializeField] private Material C_bossProjectionMat;
    private List<Door> lC_doors = new List<Door>();
    Animator C_doorAnimator = null;

    public enum RoomType
    {
        None,
        Trigger,
        Clip,
        Barrel,
        RandomModule,
        Count
    }

    [HideInInspector] public RoomType e_roomType = RoomType.None;


    // Start is called before the first frame update
    void Start()
    {
        b_locked = true;
        GetComponent<BoxCollider>().isTrigger = true;
        if (GameManager.gameManager.i_currentRoom != 7)
        {
            GetAllDoors();
            RandomiseRoomType();
        }
        else
        {
            e_roomType = RoomType.None;
        }
        UpdateDoorVisuals();
        C_coneTransform.gameObject.SetActive(false);
        C_projectionTransform.gameObject.SetActive(false);
        C_doorAnimator = GetComponentInChildren<Animator>();
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
                C_coneTransform.GetComponent<MeshRenderer>().material = C_bossConeMat;
                C_projectionTransform.GetComponent<MeshRenderer>().material = C_bossProjectionMat;
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
        AudioManager.PlayFmodEvent("SFX/Environment/Door_Open", transform.position);
        FindObjectOfType<RoomManager>().LockAllDoors();

        GameManager.gameManager.MoveToNextRoom();
    }

    public void Unlock()
    {
        b_locked = false;

        C_coneTransform.gameObject.SetActive(true);
        C_projectionTransform.gameObject.SetActive(true);
    }
    public void Lock()
    {
        b_locked = true;
    }

    public bool IsLocked()
    {
        return b_locked;
    }

    public IEnumerator PlayCloseDoorSound()
    {
        yield return new WaitForSeconds(0.9f);
        AudioManager.PlayFmodEvent("SFX/Environment/Door_Close", GameManager.GetPlayer().transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (b_locked)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            C_doorAnimator.SetBool("Door_Closed", false);
            C_doorAnimator.SetBool("Door_Open", true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (b_locked)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            C_doorAnimator.SetBool("Door_Open", false);
            C_doorAnimator.SetBool("Door_Closed", true);
        }
    }
}
