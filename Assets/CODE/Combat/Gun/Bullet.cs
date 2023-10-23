using Enemy;
using UnityEngine;
using Explosion;
using static Gun.GunModule;
using Utility;
using System.Collections.Generic;
using UnityEngine.VFX;
using Managers;

namespace Gun
{
    public class Bullet : MonoBehaviour
    {
        public struct BulletBaseInfo
        {
            [HideInInspector] public Combatant C_bulletOwner;
            [HideInInspector] public Vector3 S_firingOrigin;
            [HideInInspector] public Vector3 S_firingDirection;
            [HideInInspector] public float f_range;
            [HideInInspector] public float f_damage;
            [HideInInspector] public float f_speed;
            [HideInInspector] public float f_size;
            [HideInInspector] public float f_knockBack;
            [HideInInspector] public bool b_playerOwned;

            public BulletBaseInfo(Combatant bulletOwner, Vector3 origin, Vector3 direction, float range, float damage, float speed, float size, float knockBack)
            {
                C_bulletOwner = bulletOwner;
                S_firingOrigin = origin;
                S_firingDirection = direction;
                f_range = range;
                f_damage = damage;
                f_speed = speed;
                f_size = size;
                f_knockBack = knockBack;
                b_playerOwned = C_bulletOwner.CompareTag("Player");
            }
        }




        [HideInInspector] public BulletObjectPool C_poolOwner;
        [HideInInspector] private BulletBaseInfo S_baseInformation;
        private List<Combatant> lC_combatantsHit = new List<Combatant>();
        private Vector3 S_previousPosition;
        private float f_rotationalAcceleration;
        private float f_rotationalVelocity;
        private float f_desiredRotationAngle
        {
            get
            {
                Vector3 direction = (C_homingTarget.position - transform.position).normalized;
                return (-Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg) + 90;
            }
        }
        private float f_currentRotationAngle
        {
            get
            {
                Vector3 direction = transform.forward;
                return (-Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg) + 90;
            }
        }

        Vector3 S_hitDirection
        {
            get { return new Vector3(transform.forward.x, 0, transform.forward.z); }
        }

        private float f_bulletAliveTime;
        private float f_distanceTravelled;
        private int i_bulletPiercedCount = 0;
        private int i_ricochetCount = 0;
        private Transform C_homingTarget;


        BulletEffectInfo S_bulletEffect;
        BulletTraitInfo S_bulletTrait;

        //Visuals
        public GameObject C_hitEffect = null;
        [HideInInspector] public VisualEffect C_trailEffect;
        private BulletEffect e_lastFiredBulletEffect = BulletEffect.Count;


        void Update()
        {
            if (CheckHit())
            {
                return;
            }
            S_previousPosition = transform.position;


            //check that it hasn't gone past its range, set inactive
            if (f_bulletAliveTime > S_baseInformation.f_range || f_distanceTravelled > S_baseInformation.f_range)
            {
                C_poolOwner.MoveToOpen(this);
                return;
            }

            switch (S_bulletTrait.e_bulletTrait)
            {
                case BulletTrait.Standard:
                    // move in direction by speed, 
                    transform.position += transform.forward * S_baseInformation.f_speed * Time.deltaTime;
                    break;
                case BulletTrait.Ricochet:
                    transform.position += transform.forward * S_baseInformation.f_speed * Time.deltaTime;
                    break;
                case BulletTrait.Pierce:
                    transform.position += transform.forward * S_baseInformation.f_speed * Time.deltaTime;
                    break;
                case BulletTrait.Explosive:
                    transform.position += transform.forward * S_baseInformation.f_speed * Time.deltaTime;
                    break;
                case BulletTrait.Homing:
                    //Dot Product of thing multiplied by rad2deg multiplied by homing intensity
                    CheckHomingTarget();
                    if (f_bulletAliveTime > S_bulletTrait.f_homingDelayTime && C_homingTarget != null)
                    {

                        float angleToTarget = f_desiredRotationAngle - f_currentRotationAngle;
                        if (angleToTarget < -180)
                        {
                            angleToTarget += 360;
                        }
                        else if (angleToTarget > 180)
                        {
                            angleToTarget -= 360;
                        }

                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + angleToTarget * S_bulletTrait.f_homingStrength, 0);
                    }
                    transform.position += transform.forward * S_baseInformation.f_speed * Time.deltaTime;
                    break;

            }
            f_bulletAliveTime += Time.deltaTime;
            f_distanceTravelled += Vector3.Distance(transform.position, S_previousPosition);
        }

        public void FireBullet(Vector3 originOffset, Vector3 directionOffset, BulletBaseInfo bulletInfo, GunModule.BulletTraitInfo bulletTrait, GunModule.BulletEffectInfo bulletEffect)
        {
            f_bulletAliveTime = 0;
            f_distanceTravelled = 0;
            i_bulletPiercedCount = 0;
            i_ricochetCount = 0;
            lC_combatantsHit.Clear();

            //move bullet to closed list
            C_poolOwner.MoveToClosed(this);

            S_baseInformation = bulletInfo;

            transform.localScale = new Vector3(S_baseInformation.f_size, S_baseInformation.f_size, S_baseInformation.f_size);

            S_previousPosition = transform.position = bulletInfo.S_firingOrigin + originOffset;
            transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(-bulletInfo.S_firingDirection.z, bulletInfo.S_firingDirection.x) * Mathf.Rad2Deg + 90, 0) + directionOffset);


            S_bulletEffect = bulletEffect;
            S_bulletTrait = bulletTrait;
            if (C_trailEffect != null)
            {
                UpdateBulletTrail();
                C_trailEffect.Play();
            }
            UpdateBulletHit();
            if (S_bulletTrait.e_bulletTrait == BulletTrait.Homing)
            {
                FindHomingTarget();
            }
            e_lastFiredBulletEffect = S_bulletEffect.e_bulletEffect;
            if (S_baseInformation.b_playerOwned)
            {
                GameManager.IncrementBulletsFired();
            }
        }
        void FindHomingTarget()
        {
            if (S_baseInformation.b_playerOwned)
            {
                float closestEnemyRotation = float.MaxValue;
                int closestEnemy = int.MaxValue;
                EnemyBase[] enemiesOnScreen = FindObjectsOfType<EnemyBase>();

                for (int i = 0; i < enemiesOnScreen.Length; i++)
                {
                    Vector3 toEnemy = (transform.position - enemiesOnScreen[i].transform.position).normalized;
                    float angleToTarget = Vector3.Angle(toEnemy, transform.forward);
                    angleToTarget = -angleToTarget;

                    if (angleToTarget < closestEnemyRotation)
                    {
                        closestEnemyRotation = angleToTarget;
                        closestEnemy = i;
                    }
                }

                if (closestEnemy == int.MaxValue)
                {
                    return;
                }
                C_homingTarget = enemiesOnScreen[closestEnemy].transform;
            }
            else
            {
                PlayerController target = FindObjectOfType<PlayerController>();
                if (target != null)
                {
                    C_homingTarget = target.transform;
                    return;
                }
                C_homingTarget = null;
            }
        }

        private Transform FindRicochetTarget(Transform lastHit)
        {
            if (S_baseInformation.b_playerOwned)
            {
                float closestEnemyDistance = float.MaxValue;
                int closestEnemy = int.MaxValue;
                EnemyBase[] enemiesOnScreen = FindObjectsOfType<EnemyBase>();

                for (int i = 0; i < enemiesOnScreen.Length; i++)
                {
                    if (enemiesOnScreen[i].transform == lastHit)
                    {
                        continue;
                    }
                    Vector3 toEnemy = (transform.position - enemiesOnScreen[i].transform.position);

                    if (toEnemy.magnitude < closestEnemyDistance)
                    {
                        closestEnemyDistance = toEnemy.magnitude;
                        closestEnemy = i;
                    }
                }

                if (closestEnemy == int.MaxValue)
                {
                    return null;
                }
                return enemiesOnScreen[closestEnemy].transform;
            }
            else
            {
                PlayerController target = FindObjectOfType<PlayerController>();
                if (target != null)
                {
                    return target.transform;
                }
                return null;
            }
        }

        void CheckHomingTarget()
        {
            if (C_homingTarget != null)
            {
                if (!C_homingTarget.GetComponent<Combatant>().b_isDead)
                {
                    return;
                }
            }
            C_homingTarget = null;

            if (S_baseInformation.b_playerOwned)
            {
                FindHomingTarget();
            }
        }

        public void BulletChangeDirection(Vector3 direction)
        {
            S_baseInformation.S_firingDirection = direction;
        }
        public void BulletChangeOrigin(Vector3 origin)
        {
            S_baseInformation.S_firingOrigin = origin;
        }

        private void UpdateBulletTrail()
        {
            if (e_lastFiredBulletEffect == S_bulletEffect.e_bulletEffect)
            {
                return;
            }

            // remove unwanted trails by setting all durations to 0
            RemoveAllTrails();
            float trailSpeedModifier = S_baseInformation.f_speed / 2.0f;
            //do update on VFX magic numbers are artists numbers, modified by speed for looks
            switch (S_bulletEffect.e_bulletEffect)
            {
                case BulletEffect.None:
                    C_trailEffect.SetFloat("LeadDuration", S_baseInformation.f_range / (S_baseInformation.f_range / S_baseInformation.f_speed));
                    C_trailEffect.SetVector2("Main", new Vector2(-0.5f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector2("2nd", new Vector2(-1f, 0) * trailSpeedModifier);
                    break;
                case BulletEffect.Fire:
                    //Set duration on current effect to be alive time of bullet
                    C_trailEffect.SetFloat("FireDuration", S_baseInformation.f_range / (S_baseInformation.f_range / S_baseInformation.f_speed));
                    C_trailEffect.SetVector2("Main", new Vector2(-2, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector2("2nd", new Vector2(-0.71f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector3("Specs", new Vector3(0, 0, 1f) * trailSpeedModifier);
                    break;
                case BulletEffect.Ice:
                    //Set duration on current effect to be alive time of bullet
                    C_trailEffect.SetFloat("FrostDuration", S_baseInformation.f_range / (S_baseInformation.f_range / S_baseInformation.f_speed));
                    C_trailEffect.SetVector2("Main", new Vector2(-0.92f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector2("2nd", new Vector2(0.09f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector3("Specs", new Vector3(0, 0.2f, 0.5f) * trailSpeedModifier);
                    break;
                case BulletEffect.Lightning:
                    //Set duration on current effect to be alive time of bullet
                    C_trailEffect.SetFloat("ElectricDuration", S_baseInformation.f_range / (S_baseInformation.f_range / S_baseInformation.f_speed));
                    C_trailEffect.SetVector2("Main", new Vector2(-2, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector2("2nd", new Vector2(-1.5f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector3("Specs", new Vector3(1.5f, 0, 1.5f) * trailSpeedModifier);
                    C_trailEffect.SetVector3("Specs 2", new Vector3(-1.5f, 0, -1.5f) * trailSpeedModifier);
                    break;
                case BulletEffect.Vampire:
                    //Set duration on current effect to be alive time of bullet
                    C_trailEffect.SetFloat("LeachDuration", S_baseInformation.f_range / (S_baseInformation.f_range / S_baseInformation.f_speed));
                    C_trailEffect.SetVector2("Main", new Vector2(-1.53f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector2("2nd", new Vector2(-0.56f, 0) * trailSpeedModifier);
                    C_trailEffect.SetVector3("Specs", new Vector3(0, 0, 0.5f) * trailSpeedModifier);
                    break;
            }

        }
        private void UpdateBulletHit()
        {
            if (e_lastFiredBulletEffect == S_bulletEffect.e_bulletEffect)
            {
                return;
            }
            switch (S_bulletEffect.e_bulletEffect)
            {
                case BulletEffect.None:
                    C_hitEffect = C_poolOwner.C_gun.C_standardBulletHit;
                    break;
                case BulletEffect.Fire:
                    C_hitEffect = C_poolOwner.C_gun.C_fireBulletHit;
                    break;
                case BulletEffect.Ice:
                    C_hitEffect = C_poolOwner.C_gun.C_iceBulletHit;
                    break;
                case BulletEffect.Lightning:
                    C_hitEffect = C_poolOwner.C_gun.C_lightningBulletHit;
                    break;
                case BulletEffect.Vampire:
                    C_hitEffect = C_poolOwner.C_gun.C_vampireBulletHit;
                    break;
            }
        }
        private void SpawnHitEffect(Vector3 position)
        {
            if (C_hitEffect == null)
            {
                return;
            }
            GameObject newHit = Instantiate(C_hitEffect, position, Quaternion.Euler(-transform.forward));
            newHit.transform.localScale = new Vector3(S_baseInformation.f_size, S_baseInformation.f_size, S_baseInformation.f_size) * 4;
            if (newHit.GetComponentInChildren<VisualEffect>() != null)
            {
                newHit.GetComponentInChildren<VisualEffect>().Play();
            }
            Destroy(newHit, 0.8f);
        }

        private void RemoveAllTrails()
        {
            if (C_trailEffect == null)
            {
                return;
            }
            C_trailEffect.SetFloat("LeadDuration", 0);
            C_trailEffect.SetFloat("ElectricDuration", 0);
            C_trailEffect.SetFloat("FireDuration", 0);
            C_trailEffect.SetFloat("LeachDuration", 0);
            C_trailEffect.SetFloat("FrostDuration", 0);
        }

        private void DoBaseHit(Combatant combatant)
        {
            if (S_baseInformation.b_playerOwned)
            {
                GameManager.IncrementBulletsHit();
            }
            combatant.Damage(S_baseInformation.f_damage);
            combatant.AddVelocity(S_hitDirection * S_baseInformation.f_knockBack);
            combatant.ApplyBulletElement(S_bulletEffect, S_baseInformation);
            lC_combatantsHit.Add(combatant);
        }

        // bool returned early outs of update, if a bullet is destroyed return true else false
        private bool CheckHit()
        {
            Collider[] collisions = Physics.OverlapCapsule(S_previousPosition, transform.position, S_baseInformation.f_size, ~LayerMask.GetMask("Bullet"));
            if (collisions.Length > 0)
            {
                return OnHit(collisions[0].transform);
            }
            return false;
        }
        // bool returned early outs of update, if a bullet is destroyed return true else false
        public bool OnHit(Transform objectHit)
        {
            Combatant combatant = objectHit.GetComponent<Combatant>();

            if (combatant == null)
            {
                if (objectHit.GetComponentInParent<Destructable>() != null)
                {
                    objectHit.GetComponentInParent<Destructable>().DamageObject(S_baseInformation.f_damage);
                }
                if (S_bulletTrait.e_bulletTrait == BulletTrait.Ricochet && S_bulletTrait.i_ricochetCount >= i_ricochetCount)
                {
                    if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, S_baseInformation.f_size, ~LayerMask.GetMask("Bullet")))
                    {
                        transform.forward = Vector3.Reflect(transform.forward, hit.normal);
                        i_ricochetCount += 1;
                    }
                    return false;
                }
                else if (S_bulletTrait.e_bulletTrait == BulletTrait.Explosive)
                {
                    ExplosionGenerator.MakeExplosion(transform.position, S_bulletTrait.C_explosionPrefab, S_bulletTrait.f_explosionSize, S_bulletTrait.f_explosionDamage, S_bulletTrait.f_explosionKnockbackDistance, S_bulletTrait.f_explosionLifeTime);
                }
                SpawnHitEffect(transform.position);
                AudioManager.PlayFmodEvent("SFX/Environment/Wall_Ping", transform.position);
                C_poolOwner.MoveToOpen(this);
                return true;
            }

            for (int i = 0; i < lC_combatantsHit.Count; i++)
            {
                if (lC_combatantsHit.Contains(lC_combatantsHit[i]))
                {
                    return false;
                }
            }

            //do boolean calcs
            bool isPlayer = combatant.CompareTag("Player");
            bool isDodging = (combatant.e_combatState == Combatant.CombatState.Dodge);
            bool isInvincible = (combatant.e_combatState == Combatant.CombatState.Invincible);

            bool playerHit = isPlayer && !S_baseInformation.b_playerOwned;
            bool enemyHit = !isPlayer && S_baseInformation.b_playerOwned;

            bool shouldHit = (playerHit && (!isDodging && !isInvincible)) ||
                (enemyHit && (!isDodging && !isInvincible));


            switch (S_bulletTrait.e_bulletTrait)
            {
                case BulletTrait.Standard:
                    if (shouldHit)
                    {
                        //do damage
                        DoBaseHit(combatant);
                        SpawnHitEffect(transform.position);
                        C_poolOwner.MoveToOpen(this);

                        return true;
                    }
                    return false;
                case BulletTrait.Pierce:
                    if (shouldHit)
                    {
                        //do damage
                        DoBaseHit(combatant);
                        i_bulletPiercedCount += 1;

                        if (i_bulletPiercedCount == S_bulletTrait.i_pierceCount)
                        {
                            SpawnHitEffect(transform.position);
                            C_poolOwner.MoveToOpen(this);

                            return true;
                        }
                    }
                    return false;
                case BulletTrait.Ricochet:
                    if (shouldHit)
                    {
                        if (S_bulletTrait.i_ricochetCount >= i_ricochetCount)
                        {
                            DoBaseHit(combatant);
                            if (isPlayer)
                            {
                                i_ricochetCount += 1;
                                transform.rotation = Quaternion.Euler(new Vector3(0, ExtraMaths.FloatRandom(0, 360), 0));
                            }
                            else
                            {
                                i_ricochetCount += 1;
                                Transform newTarget = FindRicochetTarget(combatant.transform);
                                if (newTarget != null)
                                {
                                    transform.LookAt(newTarget);
                                    transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z);
                                }
                            }
                            return false;
                        }
                        else
                        {
                            DoBaseHit(combatant);
                            SpawnHitEffect(transform.position);
                            C_poolOwner.MoveToOpen(this);
                            return true;
                        }
                    }
                    return false;
                case BulletTrait.Explosive:
                    if (shouldHit)
                    {
                        DoBaseHit(combatant);
                        //create explosion with explosion size for amount of time and then
                        ExplosionGenerator.MakeExplosion(transform.position, S_bulletTrait.C_explosionPrefab, S_bulletTrait.f_explosionSize, S_bulletTrait.f_explosionDamage, S_bulletTrait.f_explosionKnockbackDistance, S_bulletTrait.f_explosionLifeTime);
                        C_poolOwner.MoveToOpen(this);
                        return true;
                    }
                    return false;
                case BulletTrait.Homing:
                    if (shouldHit)
                    {
                        DoBaseHit(combatant);
                        C_homingTarget = null;
                        SpawnHitEffect(transform.position);
                        C_poolOwner.MoveToOpen(this);
                        return true;
                    }
                    return false;
            }
            return false;

        }
    }
}
