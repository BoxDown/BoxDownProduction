using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using TMPro;
using Gun;

[RequireComponent(typeof(Animator))]
public class GunModuleUIAnimations : MonoBehaviour
{
    [Rename("Animator")] Animator C_animator;

    [Rename("Trigger Transform"), SerializeField] private Transform C_triggerTransform = null;
    [Rename("Clip Transform"), SerializeField] private Transform C_clipTransform = null;
    [Rename("Barrel Transform"), SerializeField] private Transform C_barrelTransform = null;
    [Rename("Swapping Transform"), SerializeField] private Transform C_swappingTransform = null;

    [Rename("Trigger Joint")] public Transform C_triggerJoint = null;
    [Rename("Clip Joint")] public Transform C_clipJoint = null;
    [Rename("Barrel Joint")] public Transform C_barrelJoint = null;
    [Rename("Swapping Joint"), SerializeField] private Transform C_swappingJoint = null;

    [Rename("Render Texture"), SerializeField] private CustomRenderTexture C_renderTexture;


    private void Start()
    {
        C_animator = GetComponent<Animator>();
    }

    public void PlayPauseUI()
    {
        C_animator.SetFloat("Trigger", 0.0f);
        C_animator.SetFloat("Clip", 0.0f);
        C_animator.SetFloat("Barrel", 0.0f);
        C_animator.Play("Default");
    }
    public void PlayTriggerIdle()
    {
        C_animator.Play("Default");
        C_animator.SetFloat("Trigger", 1.0f);
    }
    public void PlayClipIdle()
    {
        C_animator.Play("Default");
        C_animator.SetFloat("Clip", 1.0f);
    }
    public void PlayBarrelIdle()
    {
        C_animator.Play("Default");
        C_animator.SetFloat("Barrel", 1.0f);
    }
    public void PlayTriggerSwap()
    {
        C_animator.SetFloat("Trigger", 2.0f);
    }
    public void PlayClipSwap()
    {
        C_animator.SetFloat("Clip", 2.0f);
    }
    public void PlayBarrelSwap()
    {
        C_animator.SetFloat("Barrel", 2.0f);
    }

    public void SwapTriggerMesh(GunModule gunModule)
    {
        Destroy(C_triggerTransform.gameObject);

        C_triggerTransform = Instantiate(gunModule.C_meshPrefab).transform;
        foreach (Transform child in C_triggerTransform)
        {
            child.gameObject.layer = 8;
        }
        if (C_triggerTransform.GetComponent<Collider>() != null)
        {
            Destroy(C_triggerTransform.GetComponent<Collider>());
        }

        C_triggerTransform.parent = C_triggerJoint;
        C_triggerTransform.localPosition = Vector3.zero;
        C_triggerTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        C_triggerTransform.localScale = Vector3.one;
    }
    public void SwapClipMesh(GunModule gunModule)
    {
        Destroy(C_clipTransform.gameObject);

        C_clipTransform = Instantiate(gunModule.C_meshPrefab).transform;
        foreach (Transform child in C_clipTransform)
        {
            child.gameObject.layer = 8;
        }
        if (C_clipTransform.GetComponent<Collider>() != null)
        {
            Destroy(C_clipTransform.GetComponent<Collider>());
        }

        C_clipTransform.parent = C_clipJoint;
        C_clipTransform.localPosition = Vector3.zero;
        C_clipTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        C_clipTransform.localScale = Vector3.one;
    }
    public void SwapBarrelMesh(GunModule gunModule)
    {
        Destroy(C_barrelTransform.gameObject);

        C_barrelTransform = Instantiate(gunModule.C_meshPrefab).transform;
        foreach(Transform child in C_barrelTransform)
        {
            child.gameObject.layer = 8;
        }
        if (C_barrelTransform.GetComponent<Collider>() != null)
        {
            Destroy(C_barrelTransform.GetComponent<Collider>());
        }

        C_barrelTransform.parent = C_barrelJoint;
        C_barrelTransform.localPosition = Vector3.zero;
        C_barrelTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        C_barrelTransform.localScale = Vector3.one;
    }
    public void SwapSwappingMesh(GunModule gunModule)
    {
        Destroy(C_swappingTransform.gameObject);

        C_swappingTransform = Instantiate(gunModule.C_meshPrefab).transform;
        foreach (Transform child in C_swappingTransform)
        {
            child.gameObject.layer = 8;
        }
        if (C_swappingTransform.GetComponent<Collider>() != null)
        {
            Destroy(C_swappingTransform.GetComponent<Collider>());
        }

        C_swappingTransform.parent = C_swappingJoint;
        C_swappingTransform.localPosition = Vector3.zero;
        C_swappingTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        C_swappingTransform.localScale = Vector3.one;
    }

}