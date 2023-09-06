using Gun;
using Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using static Gun.GunModule;

public class PlayerController : Combatant
{
    /// <summary>
    /// Variables for player controller, divided into sections noted by headers
    /// </summary>
    /// 
    [Space(4)]
    [Header("Player Variables:")]
    [Space(1)]
    [Header("Control")]
    [Rename("Controller Dead Zone Percentage"), Range(0, 1), SerializeField] private float f_controllerDeadZone = 0.15f;


    [Header("Game Variables")]
    [Rename("Interact Range")] public float f_interactRange = 3.0f;
    [Rename("Spawn Location")] Vector3 S_spawnLocation; // player set to this on start and before loading into new scene


    ///<summary>
    /// Player methods, the method name should be self explanitory if not there is reference 
    ///</summary>

    protected void Start()
    {
        base.Start();
        InGameUI.gameUI.SetMaxHealth(f_maxHealth);
        InGameUI.gameUI.SetCurrentHealth(f_maxHealth);
        InGameUI.gameUI.UpdateHealthSlider();
        DontDestroyOnLoad(this);
    }

    private void Awake()
    {
        // reference control manager
        GameManager.SetPlayer(this);
        // action map control setup
        GameManager.SwitchToInGameActions();
    }
    private void OnEnable()
    {
        GameManager.SwitchToInGameActions();
    }

    private void Update()
    {
        base.Update();
        InGameUI.gameUI.UpdateHealthSlider();
    }
    private void LateUpdate()
    {
        base.LateUpdate();
        HealthUI();
        C_ownedGun.UpdateUI();
    }

    // input callbacks
    public void MoveInput(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        if (inputValue.magnitude > f_controllerDeadZone)
            ChangeMovementDirection(inputValue);
    }
    public void StopMove(InputAction.CallbackContext context)
    {
        StopMovementDirection();
    }
    public void RotationSet(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        //if control scheme mouse/keyb because player will need to rotate to mouse pos
        if (ControlManager.GetControllerType() == ControlManager.ControllerType.KeyboardMouse)
        {
            Vector3 dir = new Vector3(inputValue.x, inputValue.y, 0) - Camera.main.WorldToScreenPoint(transform.position);
            inputValue = Vector2.ClampMagnitude(new Vector2(dir.x, dir.y) / Screen.height, 1.0f);
            SetRotationDirection(inputValue);
        }
        else
        {
            if (inputValue.magnitude > f_controllerDeadZone)
                SetRotationDirection(inputValue);
        }
    }
    public void Fire(InputAction.CallbackContext context)
    {
        FireGun();
    }
    public void CancelFire(InputAction.CallbackContext context)
    {
        CancelGun();
    }
    public void Dodge(InputAction.CallbackContext context)
    {
        Dodge();

    }
    public void Interact(InputAction.CallbackContext context)
    {
        Interact();
    }
    public void Reload(InputAction.CallbackContext context)
    {
        // reload clip of bullets to max
        ReloadGun();
    }
    public void Pause(InputAction.CallbackContext context)
    {
        //bring up a menu
        if (!b_isDead)
        {
            if (PauseMenu.pauseMenu.b_gamePaused)
            {
                PauseMenu.UnpauseGame();
            }
            else
            {
                PauseMenu.PauseGame();
            }
        }
        //swap action map
    }


    public void SetPlayerPosition(Vector3 position)
    {
        transform.position = position;
    }

    private void Interact()
    {
        float closestDistance = float.MaxValue;
        int closestCollisionReference = 0;
        Collider[] collisions = Physics.OverlapSphere(transform.position, f_interactRange);
        if (collisions.Length == 0)
        {
            return;
        }
        for (int i = 0; i < collisions.Length; i++)
        {
            if (collisions[i].transform == transform)
            {
                continue;
            }
            float distance = (collisions[i].transform.position - transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollisionReference = i;
            }
        }
        Transform closestTransform = collisions[closestCollisionReference].transform;

        if (closestTransform.tag == "Gun Module")
        {
            SwapModule(closestTransform);
        }
        else if (closestTransform.tag == "Door")
        {
            if (!closestTransform.GetComponent<Door>().IsLocked())
            {
                TriggerDoor(closestTransform);
            }
        }
    }

    private void SwapModule(Transform newGunModule)
    {
        //check for interactables in radius, if none early out
        //find distance of all in radius
        //interact with shortest range

        GunModule gunModuleToSwap = (GunModule)Resources.Load(GunModuleSpawner.GetGunModuleResourcesPath(newGunModule.name));

        C_ownedGun.SwapGunPiece(gunModuleToSwap);
        Destroy(newGunModule.gameObject);
    }

    private void TriggerDoor(Transform doorGoingThrough)
    {
        doorGoingThrough.GetComponent<Door>().OnEnterDoor();
    }
    
    private void HealthUI()
    {
        InGameUI.gameUI.SetMaxHealth(f_maxHealth);
        InGameUI.gameUI.SetCurrentHealth(f_currentHealth);
        InGameUI.gameUI.UpdateHealthSlider();
    }

    //needed because we want to clean up the object pool as well
    private void OnDestroy()
    {
        DestroyImmediate(C_ownedGun.C_bulletPool.gameObject);
    }

}