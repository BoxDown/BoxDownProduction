using Gun;
using Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

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

    [HideInInspector] public Vector3 S_cameraDirection;


    ///<summary>
    /// Player methods, the method name should be self explanitory if not there is reference 
    ///</summary>

    protected void Start()
    {
        base.Start();
        InGameUI.gameUI.SetMaxHealth(f_maxHealth);
        InGameUI.gameUI.SetCurrentHealth(f_maxHealth);
        InGameUI.gameUI.UpdateHealthSlider();
        if (!GameManager.gameManager.b_debugMode)
        {
            Initialise();
        }
        DontDestroyOnLoad(this);
    }

    public void Initialise()
    {
        // reference control manager
        GameManager.SetPlayer(this);
        GameManager.SwitchToInGameActions();
    }

    private void Update()
    {
        if (S_rotationVec2Direction.magnitude < f_controllerDeadZone && S_movementVec2Direction != Vector2.zero)
        {
            SetRotationDirection(Vector2.ClampMagnitude(S_movementVec2Direction, f_controllerDeadZone * 0.9f));
        }
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
            inputValue = Vector2.ClampMagnitude((new Vector2(dir.x, dir.y) / Screen.height) * 2.5f, 1.0f);
            SetRotationDirection(inputValue);
            S_cameraDirection = new Vector3(inputValue.x, 0, inputValue.y);
        }
        else
        {
            if (inputValue.magnitude > f_controllerDeadZone)
                SetRotationDirection(inputValue);
            S_cameraDirection = new Vector3(inputValue.x, 0, inputValue.y);
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

    public override void Damage(float damage)
    {
        base.Damage(damage);
        GameManager.IncrementDamageTaken(damage);
    }
    public override void Heal(float heal)
    {
        if (f_currentHealth != f_maxHealth)
        {
            GameManager.IncrementDamageHealed(Mathf.Clamp(heal, f_maxHealth - f_currentHealth, heal));
        }
        base.Heal(heal);
    }
    protected override void Dodge()
    {
        if (i_currentDodgeCount > 0)
        {
            GameManager.IncrementDodges();
        }
        base.Dodge();        
    }
    public override void Die()
    {
        base.Die();
        GameManager.SetStopTime();
        StartCoroutine(ActivateLoseAfterSeconds(2));
    }

    public void SetPlayerPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetPlayerRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    private void Interact()
    {
        StartCoroutine(CancelDodge());
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
            if (!collisions[i].isTrigger)
            {
                continue;
            }
            float distance = Vector3.Distance(collisions[i].ClosestPoint(transform.position), transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollisionReference = i;
            }
        }

        if (collisions[closestCollisionReference].transform == transform)
        {
            return;
        }
        Transform closestTransform = collisions[closestCollisionReference].transform;

        if (closestTransform.tag == "Gun Module")
        {
            WeaponsSwapUI.Activate(GunModuleSpawner.GetGunModule(closestTransform.name), closestTransform);
        }
        else if (closestTransform.tag == "Door")
        {
            if (!closestTransform.GetComponent<Door>().IsLocked())
            {
                TriggerDoor(closestTransform);
            }
        }
    }

    public void SwapModule(Transform newGunModule)
    {
        GunModule gunModuleToSwap = GunModuleSpawner.GetGunModule(newGunModule.name);

        C_ownedGun.SwapGunPiece(gunModuleToSwap);
        Destroy(newGunModule.gameObject);
    }

    private void TriggerDoor(Transform doorGoingThrough)
    {
        ZeroVelocity();
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
        //Destroy(C_ownedGun.C_bulletPool.gameObject);
    }

    private IEnumerator ActivateLoseAfterSeconds(float time)
    {
        GameManager.SwitchToUIActions();
        yield return new WaitForSeconds(time);
        ResultsUI.ActivateLose();
    }

}