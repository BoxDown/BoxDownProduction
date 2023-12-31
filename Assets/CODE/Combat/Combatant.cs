using System.Collections;
using UnityEngine;
using Utility;
using Gun;
using System.Collections.Generic;
using UnityEngine.VFX;

public class Combatant : MonoBehaviour
{

    public enum CombatState
    {
        Normal,
        Invincible,
        Dodge,
        NoControl,
        NoAttack,
        Frozen,
        Burn,
        Count
    }

    /// <summary>
    /// Variables
    /// </summary>
    [Header("Combatant Variables:")]
    [Space(2)]
    [Header("Movement")]
    [Rename("Max Speed")] public float f_maxSpeed = 5.5f;
    [Rename("Max Acceleration")] public float f_maxAcceleration = 40;
    [Rename("Acceleration Curve")] public AnimationCurve C_accelerationCurve;
    [Space(4)]

    [Header("Rotation")]
    [Rename("Rotation Time"), Range(0, 0.5f)] public float f_rotationTime = 0.08f;
    [Space(4)]

    [Header("Dodge")]
    [Rename("Dodge Startup")] public float f_dodgeStartDelay = 0.12f;
    [Rename("Dodge Length")] public float f_dodgeLength = 2.5f;
    [Rename("Dodge Time")] public float f_dodgeTime = 0.3f;
    [Rename("Dodge Animation Curve")] public AnimationCurve C_dodgeCurve;
    protected int i_maxDodges = 1;
    [Rename("Dodge Recovery Time")] public float f_dodgeRecoveryTime;

    [Space(4)]

    [Header("Fake Physics")]
    [Rename("Size")] public float f_size = 0.35f;
    float f_stepHeight = 0.2f;
    float f_dampStrength = 0.4f;
    float f_coyoteTime = 0.8f;
    [Rename("Collision Bounce Percentage"), Range(0, 1)] public float f_collisionBounciness = 0.285f;
    [Space(4)]

    [Header("Game Variables")]
    [Rename("Max Health")] public float f_maxHealth = 100;
    [Rename("Invincibility On Hit Time")] public float f_invincibleTime = 0.15f;
    [Rename("Owned Gun")] public Gun.Gun C_ownedGun = null;
    [Rename("Debug Respawn")] public bool b_debugRespawn = false;
    [Rename("Debug Respawn Time")] public float f_respawnTime = 5.0f;
    [Space(4)]

    [Header("Visuals")]
    [Rename("After VFX")] public VisualEffect C_afterEffects;
    protected Renderer[] C_renderer;
    [Rename("Lightning Lines Materials")] public Material C_lightningMaterial;
    [Header("Dodge Visuals")]
    [Rename("Dodge Effects Particles")] public ParticleSystem C_dodgeEffects;
    [Rename("Dodge Pulse Particles")] public ParticleSystem C_dodgePulse;
    [Rename("Dodge Line Particles")] public Transform C_dodgeLine;
    private ParticleSystem[] aC_dodgeLineParticles;
    [Rename("Dodge Thrust Particles (Players)")] public ParticleSystem[] aC_dodgeThrustParticles;


    [Header("Cheats")]
    [Rename("Infinite Health")] public bool b_infiniteHealth;




    /// <summary>
    /// Run time variables
    /// </summary>
    #region RuntimeVariables
    protected float f_currentHealth;
    [HideInInspector] public CombatState e_combatState = CombatState.Normal;

    protected float f_rotationalAcceleration;
    protected float f_rotationalVelocity;
    [HideInInspector] public bool b_isDead = false;
    protected int i_bulletLayerMask;
    protected bool b_fireCancelWhileDodging;
    protected float f_currentAccelerationStep = 0;
    protected int i_currentDodgeCount;
    protected bool b_dodgeCanceled = false;
    protected bool b_lightingEffected = false;

    protected List<Transform> lC_lightningHits = new List<Transform>();

    protected bool b_hasAnimator = false;
    protected Animator C_animator = null;
    protected List<Material> C_material = new List<Material>();
    protected List<LineRenderer> lC_lineRenderers = new List<LineRenderer>();



    #region RuntimeMovement
    protected Vector2 S_movementVec2Direction;
    protected Vector2 S_movementVec2DirectionLastFrame;
    public Vector3 S_movementInputDirection
    {
        get { return new Vector3(S_movementVec2Direction.x, 0, S_movementVec2Direction.y); }
    }
    protected float f_currentMovementAngle
    {
        get { return (-Mathf.Atan2(S_velocity.normalized.z, S_velocity.normalized.x) * Mathf.Rad2Deg) + 90; }
    }

    protected Vector3 S_acceleration;
    protected Vector3 S_velocity = Vector3.zero;
    [Range(0, 1)] protected float f_slowMultiplier = 1;
    #endregion

    #region RuntimeRotation
    protected Vector2 S_previousRotationVec2Direction;
    protected float f_previousYRotationAngle
    {
        get { return ExtraMaths.Map(0, 360, -180, 180, (-Mathf.Atan2(S_previousRotationVec2Direction.y, S_previousRotationVec2Direction.x) * Mathf.Rad2Deg) + 90); }
    }
    protected Vector2 S_rotationVec2Direction;
    protected float f_desiredRotationAngle
    {
        get { return (-Mathf.Atan2(S_rotationVec2Direction.y, S_rotationVec2Direction.x) * Mathf.Rad2Deg) + 90; }
    }
    #endregion
    #endregion

    #region DebugVariable

    private float f_debugLightningSize = 0;

    #endregion

    #region UnityOverrides

    protected void Start()
    {
        i_bulletLayerMask = ~(LayerMask.GetMask("Bullet") + LayerMask.GetMask("Ignore Raycast"));
        f_currentHealth = f_maxHealth;
        i_currentDodgeCount = i_maxDodges;
        C_animator = GetComponentInChildren<Animator>();
        if (C_animator != null)
        {
            b_hasAnimator = true;
        }
        if (C_renderer != null)
        {
            return;
        }

        C_renderer = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < C_renderer.Length; i++)
        {
            C_renderer[i].material = new Material(C_renderer[i].material);
            C_material.Add(C_renderer[i].material);
        }
        C_afterEffects.transform.localPosition = new Vector3(0, f_size, 0);
        ClearEffects();

        if (C_dodgeLine != null)
        {
            aC_dodgeLineParticles = C_dodgeLine.GetComponentsInChildren<ParticleSystem>();
        }

    }

    protected virtual void Update()
    {
        if (C_ownedGun != null)
        {
            if (b_hasAnimator && C_ownedGun.b_isFiring && !b_isDead)
            {
                MoveTowardAimAnimation();
            }
            else if (b_hasAnimator && !b_isDead)
            {
                C_animator.SetFloat("Recoil", S_rotationVec2Direction.magnitude);
            }
        }
    }
    protected virtual void FixedUpdate()
    {
        if (e_combatState != CombatState.NoControl || e_combatState != CombatState.Dodge)
        {
            Move();
        }
        if (e_combatState != CombatState.NoControl)
        {
            RotateToTarget();
        }
    }

    protected virtual void LateUpdate()
    {
        S_movementVec2DirectionLastFrame = S_movementVec2Direction;
        S_previousRotationVec2Direction = S_rotationVec2Direction;
    }

    #endregion


    #region Functions  

    /// <summary>
    /// Movement Related Functions
    /// </summary>

    protected virtual void Move()
    {

        // move
        transform.localPosition += S_velocity * Time.fixedDeltaTime;

        if (b_isDead)
        {
            S_movementVec2Direction = Vector2.zero;
        }

        // if we are moving then increase the acceleration step
        if (S_movementInputDirection != Vector3.zero)
        {
            if (f_currentAccelerationStep < 1)
            {
                f_currentAccelerationStep += Time.fixedDeltaTime;
            }
        }

        float effectiveSpeed;
        //find our desired velocity and our maximum speed change
        if (C_ownedGun != null)
        {
            effectiveSpeed = Mathf.Clamp((f_maxSpeed - C_ownedGun.aC_moduleArray[1].f_movementPenalty), 0, f_maxSpeed);
        }
        else
        {
            effectiveSpeed = Mathf.Clamp((f_maxSpeed), 0, f_maxSpeed);
        }

        Vector3 desiredVelocity = S_movementInputDirection * effectiveSpeed * C_accelerationCurve.Evaluate(f_currentAccelerationStep) * f_slowMultiplier;
        float maxSpeedChange = f_maxAcceleration * Time.fixedDeltaTime;

        //move smoothly towards our desired velocity from our current veolicty
        S_velocity.x = Mathf.MoveTowards(S_velocity.x, desiredVelocity.x, maxSpeedChange);
        S_velocity.z = Mathf.MoveTowards(S_velocity.z, desiredVelocity.z, maxSpeedChange);

        S_velocity = Vector3.ClampMagnitude(S_velocity, f_maxSpeed * 2.0f);

        //animator stuff
        if (b_hasAnimator)
        {
            float strafeToSet = 0;
            float runToSet = 0;

            Vector2 aimVector = new Vector2(transform.forward.x, transform.forward.z);

            float aimBearing = 0;
            if (aimVector != Vector2.zero)
            {
                aimBearing = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
            }

            Vector2 moveVector = new Vector2(S_velocity.x, S_velocity.z);

            float moveBearing = 0;
            if (moveVector != Vector2.zero)
            {
                moveBearing = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg;
            }

            float angleDifference = aimBearing - moveBearing;

            if (angleDifference < -180) angleDifference += 360;
            if (angleDifference > 180) angleDifference -= 360;

            //angleDifference should be used to play the right animation based on quadrants

            Vector3 rotatatedVelocity = Quaternion.AngleAxis(angleDifference, Vector3.up) * Vector3.forward;

            strafeToSet = rotatatedVelocity.x * S_velocity.magnitude;
            runToSet = rotatatedVelocity.z * S_velocity.magnitude;

            C_animator.SetFloat("Strafe", strafeToSet / f_maxSpeed);
            C_animator.SetFloat("Run", runToSet / f_maxSpeed);
            if (S_velocity != Vector3.zero)
            {
                C_animator.SetBool("Movement", true);
            }
            else
            {
                C_animator.SetBool("Movement", false);
            }
        }
        CheckCollisions();

    }

    protected void ChangeMovementDirection(Vector2 input)
    {
        if (e_combatState != CombatState.Dodge && e_combatState != CombatState.NoAttack && e_combatState != CombatState.NoControl)
        {
            S_movementVec2Direction = Vector2.ClampMagnitude(input, 1f);
        }
    }

    protected void StopMovementDirection()
    {
        S_movementVec2Direction = Vector2.zero;
        f_currentAccelerationStep = 0;
        if (b_hasAnimator)
        {
            C_animator.SetFloat("Strafe", 0);
            C_animator.SetFloat("Run", 0);
            C_animator.SetBool("Movement", false);
        }
    }

    protected virtual void Dodge()
    {
        if (i_currentDodgeCount > 0)
        {
            StartCoroutine(DodgeRoutine());
        }
    }
    protected IEnumerator CancelDodge()
    {
        b_dodgeCanceled = true;
        yield return 0;
        b_dodgeCanceled = false;
    }

    public void ZeroVelocity()
    {
        S_velocity = Vector3.zero;
    }

    public void AddVelocity(Vector3 velToAdd)
    {
        if (e_combatState == CombatState.Frozen)
        {
            return;
        }

        S_velocity += velToAdd;
        CheckCollisions();
    }


    protected void RotateToTarget()
    {
        if (b_isDead)
        {
            return;
        }
        // Rotate
        transform.rotation = Quaternion.Euler(0,
            Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, f_desiredRotationAngle, ref f_rotationalVelocity, f_rotationTime),
            0);

        // based on difference between current rotation and last rotation add to rotation acceleration
        float rotationAngleDifference = Vector3.Angle(transform.forward, Quaternion.Euler(0, f_desiredRotationAngle, 0) * Vector3.forward);

        //dot product, positve/negative
        if (Vector3.Dot(transform.forward, Quaternion.Euler(0, f_desiredRotationAngle, 0) * Vector3.right) < 0)
        {
            rotationAngleDifference = -rotationAngleDifference;
        }

        // if we are close enough to our rotationn stop otherwise add velocity to our rotational velocity
        if (rotationAngleDifference <= 0.02 && rotationAngleDifference >= -0.02)
        {
            f_rotationalAcceleration = 0;
            f_rotationalVelocity = 0;
        }
        else
        {
            f_rotationalAcceleration = rotationAngleDifference * Time.fixedDeltaTime;
            f_rotationalVelocity += f_rotationalAcceleration * Time.fixedDeltaTime;
        }
    }

    protected void SetRotationDirection(Vector2 input)
    {
        S_rotationVec2Direction = Vector2.ClampMagnitude(input, 1f);
    }

    public Vector3 GetRotationDirection()
    {
        return new Vector3(S_rotationVec2Direction.x, 0, S_rotationVec2Direction.y);
    }

    public Vector3 GetRotatedVelocity()
    {
        Vector2 aimVector = new Vector2(transform.forward.x, transform.forward.z);

        float aimBearing = 0;
        if (aimVector != Vector2.zero)
        {
            aimBearing = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg;
        }

        Vector2 moveVector = new Vector2(S_velocity.x, S_velocity.z);

        float moveBearing = 0;
        if (moveVector != Vector2.zero)
        {
            moveBearing = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg;
        }

        float angleDifference = aimBearing - moveBearing;

        if (angleDifference < -180) angleDifference += 360;
        if (angleDifference > 180) angleDifference -= 360;

        //angleDifference should be used to play the right animation based on quadrants

        return Quaternion.AngleAxis(angleDifference, Vector3.up) * Vector3.forward;
    }

    protected virtual void CheckCollisions()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * f_size, f_size, Vector3.right, out hit, f_size + (S_velocity.magnitude * Time.deltaTime), i_bulletLayerMask) && S_velocity.x > 0)
        {
            if (!hit.collider.isTrigger)
            {
                S_velocity.x = -S_velocity.x * f_collisionBounciness;
            }
        }
        else if (Physics.SphereCast(transform.position + Vector3.up * f_size, f_size, -Vector3.right, out hit, f_size + (S_velocity.magnitude * Time.deltaTime), i_bulletLayerMask) && S_velocity.x < 0)
        {
            if (!hit.collider.isTrigger)
            {
                S_velocity.x = -S_velocity.x * f_collisionBounciness;
            }
        }
        if (Physics.SphereCast(transform.position + Vector3.up * f_size, f_size, Vector3.forward, out hit, f_size + (S_velocity.magnitude * Time.deltaTime), i_bulletLayerMask) && S_velocity.z > 0)
        {
            if (!hit.collider.isTrigger)
            {
                S_velocity.z = -S_velocity.z * f_collisionBounciness;
            }
        }
        else if (Physics.SphereCast(transform.position + Vector3.up * f_size, f_size, -Vector3.forward, out hit, f_size + (S_velocity.magnitude * Time.deltaTime), i_bulletLayerMask) && S_velocity.z < 0)
        {
            if (!hit.collider.isTrigger)
            {
                S_velocity.z = -S_velocity.z * f_collisionBounciness;
            }
        }
    }


    /// <summary>
    /// Damage Related Functions
    /// </summary>

    public virtual void Damage(float damage)
    {

        if (b_isDead)
        {
            return;
        }
        f_currentHealth -= damage;
        TurnOnHit();

        if (b_infiniteHealth)
        {
            StartCoroutine(HealAfterSeconds(f_invincibleTime, damage));
            return;
        }
        Invoke("TurnOffHit", f_invincibleTime);
        if (f_currentHealth <= 0)
        {
            f_currentHealth = 0;
            Die();
        }
    }

    public virtual void Heal(float heal)
    {
        if (f_currentHealth == f_maxHealth)
        {
            return;
        }

        f_currentHealth += heal;
        f_currentHealth = Mathf.Clamp(f_currentHealth, 0, f_maxHealth);
        SetHealthAmount(ExtraMaths.Map(0, 1, 0.6f, 1, (heal / f_maxHealth)));
        PlayHealEffect(1f);
        StartCoroutine(StopHealAfterSeconds(1f));
    }

    public virtual void Die()
    {
        b_isDead = true;
        ChangeState(CombatState.Normal);
        SetLightningEffected(false);
        ClearLightningHits();
        GetComponent<Collider>().enabled = false;
        StopMovementDirection();
        if (C_animator != null)
        {
            C_animator.SetFloat("Death", 1);
        }
        CancelDodge();
        if(C_ownedGun != null)
        {
            CancelGun();
        }
        ClearAllEffects();

        if (b_debugRespawn)
        {
            Invoke("Respawn", f_respawnTime);
        }
    }

    public void Respawn()
    {
        gameObject.SetActive(true);
        f_currentHealth = f_maxHealth;
        b_isDead = false;
    }

    /// <summary>
    /// Shooting Related Functions
    /// </summary>

    public void FireGun()
    {
        if (b_isDead)
        {
            return;
        }
        if (e_combatState != CombatState.Dodge && e_combatState != CombatState.NoAttack && e_combatState != CombatState.NoControl && !C_ownedGun.b_isFiring)
        {
            if (C_ownedGun.b_isFiring)
            {
                return;
            }
            C_ownedGun.StartFire();
        }
    }

    //animation trigger
    public void ShotFired()
    {
        if (b_hasAnimator)
        {
            C_animator.SetFloat("Recoil", 2);
        }
    }
    public void MoveTowardAimAnimation()
    {
        if (b_hasAnimator)
        {
            if (C_animator.GetFloat("Recoil") > 1)
            {
                C_animator.SetFloat("Recoil", Mathf.MoveTowards(C_animator.GetFloat("Recoil"), 1, (1 / C_ownedGun.f_timeBetweenBulletShots) * Time.deltaTime));
            }
        }
    }

    public void CancelGun()
    {
        if (C_ownedGun.b_isFiring)
        {
            C_ownedGun.CancelFire();
            if (e_combatState == CombatState.Dodge)
            {
                b_fireCancelWhileDodging = true;
            }
            if (b_hasAnimator)
            {
                C_animator.SetFloat("Recoil", 0);
            }
        }
    }

    public virtual void ReloadGun()
    {
        C_ownedGun.Reload();
    }

    //called in gun reload
    public void TriggerReloadAnimation()
    {
        if (b_hasAnimator)
        {
            C_animator.SetFloat("Reload", 1);
            C_animator.speed = 1 / C_ownedGun.aC_moduleArray[1].f_reloadSpeed;
            StartCoroutine(StopReloadAnimationAfterSeconds(C_ownedGun.aC_moduleArray[1].f_reloadSpeed));
        }
    }


    public IEnumerator StopReloadAnimationAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        C_animator.speed = 1;
        C_animator.SetFloat("Reload", 0);
    }

    public void SetLightningEffected(bool effected)
    {
        b_lightingEffected = effected;
        if (b_lightingEffected)
        {
            TurnOnElectric();
            return;
        }
        TurnOffElectric();
    }


    public void ApplyBulletElement(GunModule.BulletEffectInfo bulletEffectInfo, Bullet.BulletBaseInfo baseInfo)
    {
        switch (bulletEffectInfo.e_bulletEffect)
        {
            case GunModule.BulletEffect.None:
                break;
            case GunModule.BulletEffect.Fire:
                PlayFireEffect(bulletEffectInfo.f_effectTime);
                if (e_combatState != CombatState.Burn)
                {
                    TurnOnFire();
                    StartCoroutine(DamageTicksForSecond(bulletEffectInfo.i_amountOfTicks, bulletEffectInfo.f_effectTime, bulletEffectInfo.f_tickDamage));
                }
                break;
            case GunModule.BulletEffect.Ice:
                PlayIceEffect(bulletEffectInfo.f_effectTime);
                if (e_combatState == CombatState.Frozen)
                {
                    break;
                }
                if (f_slowMultiplier >= 0)
                {
                    f_slowMultiplier -= bulletEffectInfo.f_slowPercent;
                    f_slowMultiplier = Mathf.Clamp(f_slowMultiplier, 0, 1);
                    if (f_slowMultiplier == 0)
                    {
                        e_combatState = CombatState.Frozen;
                        S_velocity = Vector3.zero;
                        TurnOnFrozen();
                        SetIceAmount(1 - f_slowMultiplier);
                        StartCoroutine(ResetAfterFrozen(bulletEffectInfo.f_effectTime / bulletEffectInfo.f_slowPercent));
                        break;
                    }
                    StartCoroutine(SpeedUpAfterTime(bulletEffectInfo.f_effectTime, bulletEffectInfo.f_slowPercent));
                    SetIceAmount(1 - f_slowMultiplier);
                    break;
                }
                break;
            case GunModule.BulletEffect.Lightning:
                SetLightningEffected(true);
                PlayElectricEffect(bulletEffectInfo.f_effectTime);
                if (baseInfo.b_playerOwned)
                {
                    LightningChainCheck(bulletEffectInfo.f_chainLength, bulletEffectInfo.f_chainDamagePercent * baseInfo.f_damage, bulletEffectInfo.f_effectTime);
                }
                f_debugLightningSize = bulletEffectInfo.f_chainLength;
                if (!b_isDead)
                {
                    StartCoroutine(ClearLightningChainAfterSeconds(bulletEffectInfo.f_effectTime));
                }
                break;
            case GunModule.BulletEffect.Vampire:
                baseInfo.C_bulletOwner.Heal(baseInfo.f_damage * bulletEffectInfo.f_vampirePercent);
                TurnOnVampire();
                PlayVampireEffect(1f);
                StartCoroutine(StopVampireAfterSeconds(1f));
                break;
        }
    }

    public void ChangeState(CombatState state)
    {
        e_combatState = state;
    }

    public void LightningChainCheck(float chainRange, float chainDamage, float effectTime)
    {
        Collider[] overlaps = Physics.OverlapSphere(transform.position, chainRange);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Combatant combatant = overlaps[i].GetComponent<Combatant>();
            if (combatant != null && !combatant.CompareTag("Player"))
            {
                combatant.Damage(chainDamage);
                combatant.SetLightningEffected(true);
                StartCoroutine(combatant.ClearLightningChainAfterSeconds(effectTime));
                lC_lightningHits.Add(combatant.transform);
                //SPAWN LIGHNING EFFECT
                GameObject lightningChainObject = new GameObject();
                LineRenderer lR = lightningChainObject.AddComponent<LineRenderer>();
                lR.positionCount = 10;
                lR.startWidth = f_size * 4;
                lR.endWidth = f_size * 4 * 0.2f;
                lR.textureMode = LineTextureMode.Tile;
                for (int j = 0; j < lR.positionCount; j++)
                {
                    lR.SetPosition(j, Vector3.Lerp(transform.position, combatant.transform.position, ((j + 1) / 10)));
                }
                if (C_lightningMaterial != null)
                {
                    lR.material = C_lightningMaterial;
                }
                lC_lineRenderers.Add(lR);
            }
        }
    }

    public void ClearLightningHits()
    {
        for (int i = 0; i < lC_lineRenderers.Count; i++)
        {
            DestroyImmediate(lC_lineRenderers[i].gameObject);
        }
        lC_lineRenderers.Clear();
        lC_lightningHits.Clear();
        SetLightningEffected(false);
    }

    //Shader Functions
    #region ShaderFunctions
    public void TurnOnHit()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Hit_amount", 1);
        }
    }
    public void TurnOffHit()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Hit_amount", 0);
        }
    }

    public void TurnOnDodge()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Dodge_amount", 1);
        }
    }
    public void TurnOffDodge()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Dodge_amount", 0);
        }
    }
    public void SetHealthAmount(float healthAmount)
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Health_up_amount", healthAmount);
        }
    }
    public void TurnOffHealth()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Health_up_amount", 0);
        }
    }

    public void TurnOnFire()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Fire_Amount", 1);
        }
    }
    public void TurnOffFire()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Fire_Amount", 0);
        }
    }
    public void SetIceAmount(float iceAmount)
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Ice_amount", iceAmount);
        }
    }
    public void TurnOnFrozen()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Frozen_On_Off", 1);
        }
    }
    public void TurnOffFrozen()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Frozen_On_Off", 0);
        }
    }
    public void TurnOnElectric()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Electric_amount", 1);
        }
    }
    public void TurnOffElectric()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Electric_amount", 0);
        }
    }
    public void TurnOnVampire()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Vamp_amount", 1);
        }
    }
    public void TurnOffVampire()
    {
        foreach (Material m in C_material)
        {
            m.SetFloat("_Vamp_amount", 0);
        }
    }

    public void ClearAllEffects()
    {
        TurnOffHit();
        TurnOffDodge();
        TurnOffHealth();
        TurnOffFire();
        SetIceAmount(0);
        TurnOffFrozen();
        TurnOffElectric();
        TurnOffVampire();
    }

    #endregion

    #region AfterEffectFunctions

    private void ClearEffects()
    {
        C_afterEffects.SetFloat("IceSpawnRate", 0);
        C_afterEffects.SetFloat("FireSpawnRate", 0);
        C_afterEffects.SetFloat("ElectricSpawnRate", 0);
        C_afterEffects.SetFloat("HealingUpSpecs", 0);
        C_afterEffects.SetFloat("HealingDownSpecs", 0);
        C_afterEffects.SetFloat("HealingUpOrb", 0);
        C_afterEffects.SetFloat("HealingDownorb", 0);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
    }

    private void PlayIceEffect(float seconds)
    {
        C_afterEffects.SetFloat("IceSpawnRate", 10);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
        C_afterEffects.Play();
        Invoke("ClearIceEffect", seconds);
    }
    private void PlayFireEffect(float seconds)
    {
        C_afterEffects.SetFloat("FireSpawnRate", 16);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
        C_afterEffects.Play();
        Invoke("ClearFireEffect", seconds);
    }
    private void PlayElectricEffect(float seconds)
    {
        C_afterEffects.SetFloat("ElectricSpawnRate", 1400);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
        C_afterEffects.Play();
        Invoke("ClearElectricEffect", seconds);
    }
    private void PlayVampireEffect(float seconds)
    {
        C_afterEffects.SetFloat("HealingDownSpecs", 16);
        C_afterEffects.SetFloat("HealingDownorb", 1282);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
        C_afterEffects.Play();
        Invoke("ClearVampireEffect", seconds);
    }

    private void PlayHealEffect(float seconds)
    {
        C_afterEffects.SetFloat("HealingUpSpecs", 16);
        C_afterEffects.SetFloat("HealingUpOrb", 1282);
        C_afterEffects.SetFloat("Scale", f_size * 2.0f);
        C_afterEffects.Play();
        Invoke("ClearHealEffect", seconds);
    }

    private void ClearIceEffect()
    {
        C_afterEffects.SetFloat("IceSpawnRate", 0);
        C_afterEffects.Stop();
    }
    private void ClearFireEffect()
    {
        C_afterEffects.SetFloat("FireSpawnRate", 0);
        C_afterEffects.Stop();
    }
    private void ClearElectricEffect()
    {
        C_afterEffects.SetFloat("ElectricSpawnRate", 0);
        C_afterEffects.Stop();
    }
    private void ClearVampireEffect()
    {
        C_afterEffects.SetFloat("HealingDownSpecs", 0);
        C_afterEffects.SetFloat("HealingDownorb", 0);
        C_afterEffects.Stop();
    }
    private void ClearHealEffect()
    {
        C_afterEffects.SetFloat("HealingUpSpecs", 0);
        C_afterEffects.SetFloat("HealingUpOrb", 0);
        C_afterEffects.Stop();
    }

    #endregion

    #endregion

    #region Coroutines

    private IEnumerator HealAfterSeconds(float time, float healAmount)
    {
        yield return new WaitForSeconds(time);
        Heal(healAmount);
    }

    private IEnumerator StopVampireAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        TurnOffVampire();
    }
    private IEnumerator StopHealAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        TurnOffHealth();
    }

    private IEnumerator ClearLightningChainAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearLightningHits();
    }

    protected IEnumerator ChangeStateForSeconds(CombatState state, float seconds)
    {
        ChangeState(state);
        yield return new WaitForSeconds(seconds);
        ChangeState(CombatState.Normal);
        TurnOffHit();
    }

    //set invincible, don't let player control direction
    //move certain amount quickly
    //set state to normal
    private IEnumerator DodgeRoutine()
    {
        //Pre Dodge, if no input direction do nothing, delay dodge before use

        if (S_movementInputDirection == Vector3.zero)
        {
            yield break;
        }
        yield return new WaitForSeconds(f_dodgeStartDelay);
        i_currentDodgeCount--;

        //Audio And Visual Setup of Dodge
        bool firingAtStartOfDodge = C_ownedGun.b_isFiring;
        if (firingAtStartOfDodge)
        {
            C_ownedGun.CancelFire();
        }
        AudioManager.PlayFmodEvent("SFX/PlayerDash", transform.position);

        //Implementing Effects For Player
        if (CompareTag("Player"))
        {
            //Effects To Implement
            //C_dodgeEffects            C_dodgeLines            C_dodgeThrustParticles
            C_dodgeEffects.Play();
            C_dodgePulse.Play();

            C_dodgeLine.localPosition = -GetRotatedVelocity().normalized + (Vector3.up * 0.3f);
            for (int i = 0; i < aC_dodgeThrustParticles.Length; i++)
            {
                aC_dodgeThrustParticles[i].startRotation = ((f_desiredRotationAngle + 90) - 15 + (i * 30)) * Mathf.Deg2Rad;
                aC_dodgeThrustParticles[i].Play();
            }
            //C_dodgeLine.localRotation = Quaternion.Euler(new Vector3(0, -transform.rotation.eulerAngles.y, 0));
            foreach (ParticleSystem pS in aC_dodgeLineParticles)
            {
                pS.startRotation3D = new Vector3(pS.startRotation3D.x, ((f_currentMovementAngle + 90) * Mathf.Deg2Rad), pS.startRotation3D.z);
                pS.Play();
            }
        }

        //Visual
        TurnOnDodge();
        if (C_animator != null)
        {
            C_animator.SetBool("Dodge", true);
        }

        //actually dodge stuff
        ChangeState(CombatState.Dodge);
        Vector3 startPosition = transform.position;
        float dodgeDistance = f_dodgeLength * f_slowMultiplier;
        float dodgeTime = f_dodgeTime;
        float timeSinceStart = 0;
        Vector3 goalPosition = Vector3.zero;

        //What needs to happen: We need to be able to dodge through certain objects, anything on a certain layer and any enemies
        //calculate goal position
        RaycastHit hit;
        //cast to see what I am about to hit
        if (Physics.SphereCast(transform.position + (Vector3.up * f_size * 1.1f), f_size, S_movementInputDirection, out hit, dodgeDistance, i_bulletLayerMask))
        {
            //if I hit the dodge layer
            if (hit.transform.gameObject.layer == 9)
            {
                //check for collisions at the target destination, if none, set the target destination
                Collider[] collisions = Physics.OverlapSphere(transform.position + (S_movementInputDirection.normalized * dodgeDistance) + (Vector3.up * f_size * 1.1f), f_size, i_bulletLayerMask);
                bool isCollidingWithWall = false;
                bool isCollidingWithDodgeObject = false;

                for (int i = 0; i < collisions.Length; i++)
                {
                    if (collisions[i].transform.gameObject.layer == 9)
                    {
                        isCollidingWithDodgeObject = true;
                    }
                    else
                    {
                        isCollidingWithWall = true;
                    }
                }

                if (isCollidingWithWall)
                {
                    Physics.SphereCast(transform.position + (S_movementInputDirection.normalized * dodgeDistance) + (Vector3.up * f_size * 1.1f), f_size, -S_movementInputDirection, out RaycastHit backHit, dodgeDistance, i_bulletLayerMask);
                    if (backHit.transform.gameObject.layer == 9)
                    {
                        goalPosition = transform.position + (S_movementInputDirection.normalized * (dodgeDistance - backHit.distance));
                    }
                    else
                    {
                        dodgeDistance = hit.distance - f_size;
                        float dodgePercentage = dodgeDistance / f_dodgeLength;
                        dodgeTime = f_dodgeTime * dodgePercentage;
                        goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
                    }
                }
                else if (isCollidingWithDodgeObject)
                {
                    Collider[] newLocationCollisions = Physics.OverlapSphere(transform.position + (S_movementInputDirection.normalized * dodgeDistance * 1.5f) + (Vector3.up * f_size * 1.1f), f_size, i_bulletLayerMask);
                    if (newLocationCollisions.Length == 0)
                    {
                        Physics.SphereCast(transform.position + (S_movementInputDirection.normalized * dodgeDistance) + (Vector3.up * f_size * 1.1f), f_size, -S_movementInputDirection, out RaycastHit backHit, dodgeDistance, i_bulletLayerMask);
                        if (backHit.transform.gameObject.layer == 9)
                        {
                            goalPosition = transform.position + (S_movementInputDirection.normalized * (dodgeDistance - backHit.distance));
                        }
                        else
                        {
                            dodgeDistance = hit.distance - f_size;
                            float dodgePercentage = dodgeDistance / f_dodgeLength;
                            dodgeTime = f_dodgeTime * dodgePercentage;
                            goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
                        }
                    }
                    else
                    {
                        dodgeDistance = hit.distance - f_size;
                        float dodgePercentage = dodgeDistance / f_dodgeLength;
                        dodgeTime = f_dodgeTime * dodgePercentage;
                        goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
                    }
                }
                else
                {
                    goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
                }
            }
            //if I hit an enemy
            else if (hit.transform.GetComponent<Combatant>() != null)
            {
                float remainingDodgeDistance = f_dodgeLength - hit.distance;
                if (Physics.SphereCast(hit.point, f_size, S_movementInputDirection, out RaycastHit wallCheck, remainingDodgeDistance, i_bulletLayerMask))
                {
                    dodgeDistance = (hit.distance + wallCheck.distance) - f_size;
                    float dodgePercentage = dodgeDistance / f_dodgeLength;
                    dodgeTime = f_dodgeTime * dodgePercentage;
                }
                goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
            }
            //if I hit anything else
            else
            {
                dodgeDistance = hit.distance - f_size;
                float dodgePercentage = dodgeDistance / f_dodgeLength;
                dodgeTime = f_dodgeTime * dodgePercentage;
                goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
            }
        }
        else
        {
            goalPosition = transform.position + (S_movementInputDirection.normalized * dodgeDistance);
        }

        while (dodgeTime > timeSinceStart && !b_dodgeCanceled)
        {
            transform.position = Vector3.Lerp(startPosition, goalPosition, C_dodgeCurve.Evaluate(timeSinceStart / dodgeTime));
            yield return 0;
            timeSinceStart += Time.deltaTime;
        }

        ChangeState(CombatState.Normal);
        if (!b_fireCancelWhileDodging && firingAtStartOfDodge)
        {
            C_ownedGun.StartFire();
        }
        b_fireCancelWhileDodging = false;
        b_dodgeCanceled = false;
        TurnOffDodge();
        if (C_animator != null)
        {
            C_animator.SetBool("Dodge", false);
        }
        StartCoroutine(StartDodgeRecovery());
    }

    protected IEnumerator StartDodgeRecovery()
    {
        float startTime = Time.time;

        while (Time.time - startTime < f_dodgeRecoveryTime)
        {
            yield return 0;
        }
        i_currentDodgeCount++;
    }


    protected IEnumerator SpeedUpAfterTime(float effectTime, float increaseAmount)
    {
        yield return new WaitForSeconds(effectTime);
        if (e_combatState != CombatState.Frozen)
        {
            f_slowMultiplier += increaseAmount;
        }
        else
        {
            f_slowMultiplier = 0;
        }
        Mathf.Clamp01(f_slowMultiplier);
        SetIceAmount(1 - f_slowMultiplier);
    }

    protected IEnumerator ResetAfterFrozen(float effectTime)
    {
        yield return new WaitForSeconds(effectTime);
        S_velocity = Vector3.zero;
        f_slowMultiplier = 1;
        ChangeState(CombatState.Normal);
        SetIceAmount(1 - f_slowMultiplier);
        TurnOffFrozen();
    }

    protected IEnumerator DamageTicksForSecond(int tickCount, float seconds, float damage)
    {
        float startTime = Time.time;
        float timeToWaitForTicks = seconds / (float)tickCount;

        while (Time.time - startTime < seconds)
        {
            Damage(damage);
            yield return new WaitForSeconds(timeToWaitForTicks);
        }
        TurnOffFire();
        yield return null;
    }

    #endregion

    private void OnDrawGizmos()
    {
        //lightning gizmo
        Gizmos.color = Color.red;
        foreach (Transform t in lC_lightningHits)
        {
            Gizmos.DrawLine(transform.position, t.position);
        }
        if (b_lightingEffected)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0.01f, 1));
            Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
            Gizmos.DrawSphere(Vector3.zero, f_debugLightningSize);
        }
    }

}
