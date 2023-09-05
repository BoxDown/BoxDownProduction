using Gun;
using Managers;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using Utility;
using static Gun.GunModule;

[RequireComponent(typeof(PlayerInput))]
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
    private PlayerInput C_playerInput;


    [Header("Game Variables")]
    [Rename("Interact Range")] public float f_interactRange = 3.0f;
    [Rename("Spawn Location")] Vector3 S_spawnLocation; // player set to this on start and before loading into new scene

    //run time
    private ControlManager C_controlManagerReference = null;

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
        C_controlManagerReference = FindObjectOfType<ControlManager>();

        // action map control setup
        C_playerInput = GetComponent<PlayerInput>();
        C_playerInput.SwitchCurrentActionMap("PlayerControl");
        EnableInGameActions();
    }
    private void OnEnable()
    {
        EnableInGameActions();
    }

    private void EnableInGameActions()
    {
        C_playerInput.SwitchCurrentActionMap("PlayerControl");
        InputActionMap actionMap = C_playerInput.currentActionMap;
        actionMap.Enable();
        actionMap.FindAction("Movement").performed += MoveInput;
        actionMap.FindAction("Movement").canceled += StopMove;
        actionMap.FindAction("Rotate").performed += RotationSet;
        actionMap.FindAction("Dodge").performed += Dodge;
        actionMap.FindAction("Interact").performed += Interact;
        actionMap.FindAction("Fire").performed += Fire;
        actionMap.FindAction("Fire").canceled += CancelFire;
        actionMap.FindAction("Reload").performed += Reload;
        actionMap.FindAction("Pause").performed += Pause;
    }

    private void Update()
    {
        base.Update();
        C_controlManagerReference.ChangeInputDevice(C_playerInput.currentControlScheme);
        InGameUI.gameUI.UpdateHealthSlider();
    }
    private void LateUpdate()
    {
        base.LateUpdate();
        HealthUI();
        C_ownedGun.UpdateUI();
    }

    // input callbacks
    private void MoveInput(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        if (inputValue.magnitude > f_controllerDeadZone)
            ChangeMovementDirection(inputValue);
    }
    private void StopMove(InputAction.CallbackContext context)
    {
        StopMovementDirection();
    }
    private void RotationSet(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        //if control scheme mouse/keyb because player will need to rotate to mouse pos
        if (C_controlManagerReference.GetCurrentControllerType() == ControlManager.CurrentControllerType.KeyboardMouse)
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
    private void Fire(InputAction.CallbackContext context)
    {
        FireGun();
    }
    private void CancelFire(InputAction.CallbackContext context)
    {
        CancelGun();
    }
    private void Dodge(InputAction.CallbackContext context)
    {
        Dodge();

    }
    private void Interact(InputAction.CallbackContext context)
    {
        Interact();
    }
    private void Reload(InputAction.CallbackContext context)
    {
        // reload clip of bullets to max
        ReloadGun();
    }
    private void Pause(InputAction.CallbackContext context)
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

}