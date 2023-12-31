using Managers;
using System.Collections;
using System.Linq;
using UnityEngine;
using Utility;
using Enemy;

namespace Gun
{
    public class Gun : MonoBehaviour
    {
        /// <summary>
        /// Reflection of stats inside of gun module
        /// </summary>

        #region GunModuleReflection
        //Trigger Group
        float f_baseDamage;
        float f_fireRate;
        float f_bulletSpeed;
        float f_knockBack;
        GunModule.BulletTraitInfo S_bulletTraitInfo;

        //Clip Group
        float f_reloadSpeed;
        float f_movementPenalty;
        int i_clipSize;
        GunModule.BulletEffectInfo S_bulletEffectInfo;


        //Barrel Group
        GunModule.ShotPattern e_shotPattern;
        float f_bulletSize;
        float f_bulletRange;
        float f_recoil;
        bool b_burstTrue;
        int i_burstCount;
        float f_burstInterval;

        GunModule.ShotPatternInfo S_shotPatternInfo;
        #endregion


        [Rename("Muzzle Transform")] public Transform C_muzzle;
        [Rename("Muzzle Light Flash")] public Light C_light;
        [Rename("Light Off Time")] public float f_lightOffTime = 0.024f;
        [Space(10)]

        [Header("LEAVE NULL UNLESS PLAYER")]
        [Rename("Trigger Transform")] public Transform C_trigger;
        [Rename("Clip Transform")] public Transform C_clip;
        [Rename("Barrel Transform")] public Transform C_barrel;

        [Rename("Trigger Joint")] public Transform C_triggerJoint;
        [Rename("Clip Joint")] public Transform C_clipJoint;
        [Rename("Barrel Joint")] public Transform C_barrelJoint;



        [Space(10)]

        [Rename("Debug Gun")] public bool b_debugGun = false;

        private Vector3 S_muzzlePosition
        {
            get { return C_muzzle == null ? transform.position : C_muzzle.position; }
        }

        [Rename("Gun Holder")] public Combatant C_gunHolder;
        [Rename("Gun Modules")] public GunModule[] aC_moduleArray = new GunModule[3];

        float f_lastFireTime = 0;
        float f_timeSinceLastFire { get { return Time.time - f_lastFireTime; } }
        [HideInInspector] public float f_timeBetweenBulletShots { get { return 1.0f / f_fireRate; } }
        float f_timeUntilNextFire = 0;

        int i_currentAmmo;

        [HideInInspector] public bool b_isFiring = false;
        float f_fireHoldTime = 0;
        bool b_reloadCancel = false;
        bool b_reloading = false;
        [HideInInspector] public BulletObjectPool C_bulletPool;

        [Header("Bullet Colours")]
        [Rename("Standard Colour"), ColorUsage(true, true)] public Color S_standardColour = new Color(0.75f, 0.5f, 0.2f, 1);
        [Rename("Fire Colour"), ColorUsage(true, true)] public Color S_fireColour = new Color(1f, 0.2f, 0f, 1);
        [Rename("Ice Colour"), ColorUsage(true, true)] public Color S_iceColour = new Color(0.35f, 0.8f, 0.7f, 1);
        [Rename("Lightning Colour"), ColorUsage(true, true)] public Color S_lightningColour = new Color(1f, 1f, 0.25f, 1);
        [Rename("Vampire Colour"), ColorUsage(true, true)] public Color S_vampireColour = new Color(0.5f, 0.8f, 0.1f, 1);

        [Header("Bullet Material")]
        [Rename("Standard Bullet Material")] public Material C_standardBulletMaterial;
        [Rename("Fire Bullet Material")] public Material C_fireBulletMaterial;
        [Rename("Ice Bullet Material")] public Material C_iceBulletMaterial;
        [Rename("Electric Bullet Material")] public Material C_lightningBulletMaterial;
        [Rename("Vampire Bullet Material")] public Material C_vampireBulletMaterial;


        [Space(15)]
        [Header("Bullet Shapes")]
        [Rename("Standard Bullet Mesh")] public Mesh C_standardMesh;
        [Rename("Pierce Bullet Mesh")] public Mesh C_pierceMesh;
        [Rename("Ricochet Bullet Mesh")] public Mesh C_ricochetMesh;
        [Rename("Explosive Bullet Mesh")] public Mesh C_explosiveMesh;
        [Rename("Homing Bullet Mesh")] public Mesh C_homingMesh;

        [Space(15)]
        [Header("VFX")]
        [Rename("Bullet Shells")] public GameObject C_bulletShells;
        [Rename("Bullet Shell Spawn Location")] public Transform C_bulletShellSpawn;
        [Rename("Bullet Standard Hit Effect")] public GameObject C_standardBulletHit;
        [Rename("Bullet Fire Hit Effect")] public GameObject C_fireBulletHit;
        [Rename("Bullet Ice Hit Effect")] public GameObject C_iceBulletHit;
        [Rename("Bullet Lightning Hit Effect")] public GameObject C_lightningBulletHit;
        [Rename("Bullet Vampire Hit Effect")] public GameObject C_vampireBulletHit;

        [Header("Muzzle VFX")]
        [Rename("Muzzle Particle System")] public ParticleSystem C_muzzleFlashParticle;
        [Rename("Standard Muzzle Material")] public Material C_standardMuzzleMaterial;
        [Rename("Fire Muzzle Material")] public Material C_fireMuzzleMaterial;
        [Rename("Ice Muzzle Material")] public Material C_iceMuzzleMaterial;
        [Rename("Electric Muzzle Material")] public Material C_electricMuzzleMaterial;
        [Rename("Vampire Muzzle Material")] public Material C_vampireMuzzleMaterial;
        Bullet.BulletBaseInfo S_bulletInfo { get { return new Bullet.BulletBaseInfo(C_gunHolder, S_muzzlePosition, C_muzzle == null ?  transform.forward : C_muzzle.transform.up, f_bulletRange, f_baseDamage, f_bulletSpeed, f_bulletSize, f_knockBack); ; } }


        public void InitialiseGun()
        {
            for (int i = 0; i < aC_moduleArray.Count(); i++)
            {
                UpdateGunStats(aC_moduleArray[i]);
            }
            C_bulletPool = GameManager.GetBulletPool();
            i_currentAmmo = i_clipSize;
        }

        private void Update()
        {
            if (b_isFiring && !b_reloading)
            {
                f_timeUntilNextFire -= Time.deltaTime;
                f_fireHoldTime += Time.deltaTime;
                Fire();
            }
        }
        private void FixedUpdate()
        {
            if (b_debugGun)
            {
                for (int i = 0; i < aC_moduleArray.Count(); i++)
                {
                    UpdateGunStats(aC_moduleArray[i]);
                }
            }
        }
        public void StartFire()
        {
            C_gunHolder.ShotFired();
            f_timeUntilNextFire = f_timeBetweenBulletShots;
            b_isFiring = true;
        }
        private void Fire()
        {
            if (f_timeSinceLastFire < f_timeBetweenBulletShots || b_reloading)
            {
                f_timeUntilNextFire = 0;
                return;
            }
            if (i_currentAmmo <= 0 || i_currentAmmo < i_burstCount)
            {
                Reload();
                f_timeUntilNextFire = 0;
                return;
            }
            int timesFiredThisFrame = 0;
            while (f_timeUntilNextFire < 0.0f)
            {
                float timeIntoNextFrame = -f_timeUntilNextFire;
                //Spawn Bullet, at muzzle position + (bullet trajectory * bulletspeed) * time into next frame
                if (!b_burstTrue)
                {
                    switch (S_shotPatternInfo.e_shotPattern)
                    {
                        case GunModule.ShotPattern.Straight:
                            FireStraight(timeIntoNextFrame);
                            C_gunHolder.ShotFired();
                            break;
                        case GunModule.ShotPattern.Multishot:
                            //will need coroutine if you don't do something clever. Think.
                            FireMultiShot(timeIntoNextFrame);
                            C_gunHolder.ShotFired();
                            break;
                        case GunModule.ShotPattern.Buckshot:
                            FireBuckShot(timeIntoNextFrame);
                            C_gunHolder.ShotFired();
                            break;
                        case GunModule.ShotPattern.Spray:
                            FireSpray(timeIntoNextFrame);
                            C_gunHolder.ShotFired();
                            break;
                        case GunModule.ShotPattern.Wave:
                            FireWave(timeIntoNextFrame);
                            C_gunHolder.ShotFired();
                            break;
                    }
                }
                timesFiredThisFrame += 1;
                Vector3 recoil = -C_gunHolder.transform.forward * Mathf.Clamp(f_recoil - f_movementPenalty, 0, f_recoil);
                if (C_light)
                {
                    TurnOnLight();
                }
                if (C_muzzleFlashParticle)
                {
                    C_muzzleFlashParticle.startRotation = (C_gunHolder.transform.rotation.eulerAngles.y - 90) * Mathf.Deg2Rad;
                    C_muzzleFlashParticle.Play();
                }

                if (C_gunHolder.CompareTag("Player"))
                {
                    GameManager.GetCamera().GunshotCameraShake();
                    InGameUI.gameUI.BulletFireUI();
                    AudioManager.FireBulletSound(S_bulletEffectInfo.e_bulletEffect, S_muzzlePosition);
                }
                else if(C_gunHolder.transform.GetComponent<Mite>() != null || C_gunHolder.transform.GetComponent<Wasp>() != null)
                {
                    AudioManager.PlayFmodEvent("SFX/SmallEnemyShot", S_muzzlePosition);
                }
                else
                {
                    AudioManager.PlayFmodEvent("SFX/SLargeEnemyShot", S_muzzlePosition);
                }
                C_gunHolder.GetComponent<Combatant>().AddVelocity(recoil);
                SpawnBulletShells();

                f_timeUntilNextFire += f_timeBetweenBulletShots;
            }

            f_lastFireTime = Time.time;
            i_currentAmmo -= timesFiredThisFrame;
        }
        public void CancelFire()
        {
            f_fireHoldTime = 0;
            f_timeUntilNextFire = 0;
            b_isFiring = false;
        }
        public void Reload()
        {
            // read clip size and current bullet count and reload time
            // reload 1 at a time,
            //optional cancelleable reload
            if (i_currentAmmo < i_clipSize)
            {
                StartCoroutine(ReloadAfterTime());
                StartReloadAnimation();
            }
        }
        private void StartReloadAnimation()
        {
            C_gunHolder.TriggerReloadAnimation();
        }
        public void HardReload()
        {
            i_currentAmmo = i_clipSize;
            if (C_gunHolder.CompareTag("Player"))
            {
                InGameUI.gameUI.HardReloadUI();
            }
        }

        /// <summary>
        /// Updates Variables for gun dependent on the gun modules type.
        /// </summary>
        public void UpdateGunStats(GunModule gunModule)
        {
            //only update stats that we change

            switch (gunModule.e_moduleType)
            {
                case GunModule.ModuleSection.Trigger:
                    UpdateTriggerStats(gunModule);
                    aC_moduleArray[(int)GunModule.ModuleSection.Trigger] = gunModule;
                    break;
                case GunModule.ModuleSection.Clip:
                    UpdateClipStats(gunModule);
                    aC_moduleArray[(int)GunModule.ModuleSection.Clip] = gunModule;
                    if (C_gunHolder.CompareTag("Player"))
                    {
                        InGameUI.gameUI.UpdateBulletCount(aC_moduleArray[1].i_clipSize);
                    }
                    break;
                case GunModule.ModuleSection.Barrel:
                    UpdateBarrelStats(gunModule);
                    aC_moduleArray[(int)GunModule.ModuleSection.Barrel] = gunModule;
                    break;
            }
            HardReload();
        }
        private void UpdateTriggerStats(GunModule gunModule)
        {
            if (gunModule.e_moduleType != GunModule.ModuleSection.Trigger)
            {
                return;
            }

            if (C_gunHolder.CompareTag("Player"))
            {
                Destroy(C_trigger.gameObject);

                C_trigger = Instantiate(gunModule.C_meshPrefab).transform;
                C_trigger.gameObject.tag = "Player";
                if (C_trigger.GetComponent<Collider>() != null)
                {
                    Destroy(C_trigger.GetComponent<Collider>());
                }

                Destroy(C_trigger.Find("ModuleEffects").gameObject);


                C_trigger.parent = C_triggerJoint;
                C_trigger.localPosition = Vector3.zero;
                C_trigger.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                C_trigger.localScale = Vector3.one;
            }

            f_baseDamage = gunModule.f_baseDamage;
            f_fireRate = gunModule.f_fireRate;
            f_bulletSpeed = gunModule.f_bulletSpeed;
            f_knockBack = gunModule.f_knockBack;

            S_bulletTraitInfo = gunModule.S_bulletTraitInformation;

        }
        private void UpdateClipStats(GunModule gunModule)
        {
            if (gunModule.e_moduleType != GunModule.ModuleSection.Clip)
            {
                return;
            }


            if (C_gunHolder.CompareTag("Player"))
            {
                Destroy(C_clip.gameObject);

                C_clip = Instantiate(gunModule.C_meshPrefab).transform;
                C_clip.gameObject.tag = "Player";
                if (C_clip.GetComponent<Collider>() != null)
                {
                    Destroy(C_clip.GetComponent<Collider>());
                }

                Destroy(C_clip.Find("ModuleEffects").gameObject);


                C_clip.parent = C_clipJoint;
                C_clip.localPosition = Vector3.zero;
                C_clip.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                C_clip.localScale = Vector3.one;
            }

            f_reloadSpeed = gunModule.f_reloadSpeed;
            f_movementPenalty = gunModule.f_movementPenalty;
            i_clipSize = gunModule.i_clipSize;

            S_bulletEffectInfo = gunModule.S_bulletEffectInformation;

            ChangeMuzzleFlashMaterial(S_bulletEffectInfo.e_bulletEffect);
            if (C_light == null)
            {
                return;
            }
            switch (S_bulletEffectInfo.e_bulletEffect)
            {
                case GunModule.BulletEffect.None:
                    ChangeLightColour(S_standardColour);
                    break;
                case GunModule.BulletEffect.Fire:
                    ChangeLightColour(S_fireColour);
                    break;
                case GunModule.BulletEffect.Ice:
                    ChangeLightColour(S_iceColour);
                    break;
                case GunModule.BulletEffect.Lightning:
                    ChangeLightColour(S_lightningColour);
                    break;
                case GunModule.BulletEffect.Vampire:
                    ChangeLightColour(S_vampireColour);
                    break;
            }
        }
        private void UpdateBarrelStats(GunModule gunModule)
        {
            if (gunModule.e_moduleType != GunModule.ModuleSection.Barrel)
            {
                return;
            }

            if (C_gunHolder.CompareTag("Player"))
            {
                Destroy(C_barrel.gameObject);

                C_barrel = Instantiate(gunModule.C_meshPrefab).transform;
                C_barrel.gameObject.tag = "Player";
                if (C_barrel.GetComponent<Collider>() != null)
                {
                    Destroy(C_barrel.GetComponent<Collider>());
                }

                Destroy(C_barrel.Find("ModuleEffects").gameObject);

                C_barrel.parent = C_barrelJoint;
                C_barrel.localPosition = Vector3.zero;
                C_barrel.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                C_barrel.localScale = Vector3.one;
            }

            f_bulletSize = gunModule.f_bulletSize;
            f_bulletRange = gunModule.f_bulletRange;
            f_recoil = gunModule.f_recoil;

            S_shotPatternInfo = gunModule.S_shotPatternInformation;
        }

        /// <summary>
        /// Firing Support Functions
        /// </summary>
        private void FireStraight(float timeIntoNextFrame)
        {
            C_bulletPool.GetFirstOpen().FireBullet(S_bulletInfo.S_firingDirection * timeIntoNextFrame, Vector3.zero, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
        }
        private void FireMultiShot(float timeIntoNextFrame)
        {
            for (int i = 0; i < S_shotPatternInfo.i_shotCount; i++)
            {
                C_bulletPool.GetFirstOpen().FireBullet(S_bulletInfo.S_firingDirection * timeIntoNextFrame + (transform.right * (-S_shotPatternInfo.f_multiShotDistance * (S_shotPatternInfo.i_shotCount - 1)) / 2.0f) + (transform.right * (i * (S_shotPatternInfo.f_multiShotDistance))),
                    Vector3.zero, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
            }
        }
        private void FireBuckShot(float timeIntoNextFrame)
        {
            Vector3 fireAngle;
            if (S_shotPatternInfo.b_randomSpread)
            {
                for (int i = 0; i < S_shotPatternInfo.i_shotCount; i++)
                {
                    fireAngle = new Vector3(0, ExtraMaths.FloatRandom(-S_shotPatternInfo.f_maxAngle, S_shotPatternInfo.f_maxAngle), 0);
                    C_bulletPool.GetFirstOpen().FireBullet((S_bulletInfo.S_firingDirection) * timeIntoNextFrame, fireAngle, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
                }
            }
            else
            {
                for (int i = 0; i < S_shotPatternInfo.i_shotCount; i++)
                {
                    fireAngle = new Vector3(0, -S_shotPatternInfo.f_maxAngle + (i * (2 * S_shotPatternInfo.f_maxAngle / (float)(S_shotPatternInfo.i_shotCount - 1))), 0);
                    C_bulletPool.GetFirstOpen().FireBullet((S_bulletInfo.S_firingDirection) * timeIntoNextFrame, fireAngle, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
                }
            }
        }
        private void FireSpray(float timeIntoNextFrame)
        {
            Vector3 fireAngle = new Vector3(0, Random.Range(-S_shotPatternInfo.f_maxAngle, S_shotPatternInfo.f_maxAngle), 0);
            C_bulletPool.GetFirstOpen().FireBullet((S_bulletInfo.S_firingDirection) * timeIntoNextFrame, fireAngle, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
        }
        private void FireWave(float timeIntoNextFrame)
        {
            Vector3 fireAngle = new Vector3(0, ExtraMaths.Map(-1, 1, -S_shotPatternInfo.f_maxAngle, S_shotPatternInfo.f_maxAngle, Mathf.Sin(f_fireHoldTime * (Mathf.PI))), 0);
            C_bulletPool.GetFirstOpen().FireBullet((S_bulletInfo.S_firingDirection) * timeIntoNextFrame, fireAngle, S_bulletInfo, S_bulletTraitInfo, S_bulletEffectInfo, this);
        }

        //swap gun pieces to be in correct order when
        private void SortModules()
        {

        }

        public void SwapGunPiece(GunModule newModule)
        {
            GunModule oldModule = null;
            string oldModuleToSpawn = "";
            switch (newModule.e_moduleType)
            {
                case GunModule.ModuleSection.Trigger:
                    oldModule = aC_moduleArray[0];
                    oldModuleToSpawn = $"Trigger\\{oldModule.name}";
                    aC_moduleArray[0] = newModule;
                    break;
                case GunModule.ModuleSection.Clip:
                    oldModule = aC_moduleArray[1];
                    oldModuleToSpawn = $"Clip\\{oldModule.name}";
                    aC_moduleArray[1] = newModule;
                    break;
                case GunModule.ModuleSection.Barrel:
                    oldModule = aC_moduleArray[2];
                    oldModuleToSpawn = $"Barrel\\{oldModule.name}";
                    aC_moduleArray[2] = newModule;
                    break;
            }
            GunModuleSpawner.SpawnGunModule(oldModuleToSpawn, new Vector3(transform.position.x, 0, transform.position.z));
            UpdateGunStats(newModule);
        }

        public void ResetToBaseStats()
        {
            GunModule baseBarrel = (GunModule)Resources.Load($"GunModules\\Barrel\\BaseBarrel");
            GunModule baseClip = (GunModule)Resources.Load($"GunModules\\Clip\\BaseClip");
            GunModule baseTrigger = (GunModule)Resources.Load($"GunModules\\Trigger\\BaseTrigger");
            UpdateGunStats(baseBarrel);
            UpdateGunStats(baseClip);
            UpdateGunStats(baseTrigger);
        }

        private void TurnOnLight()
        {
            C_light.gameObject.SetActive(true);
            StartCoroutine(TurnOffLight());
        }

        private void ChangeMuzzleFlashMaterial(GunModule.BulletEffect bulletEffect)
        {
            if(C_muzzleFlashParticle == null)
            {
                return;
            }
            switch (bulletEffect)
            {
                case GunModule.BulletEffect.None:
                    C_muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = C_standardMuzzleMaterial;
                    break;
                case GunModule.BulletEffect.Fire:
                    C_muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = C_fireMuzzleMaterial;
                    break;
                case GunModule.BulletEffect.Ice:
                    C_muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = C_iceMuzzleMaterial;
                    break;
                case GunModule.BulletEffect.Lightning:
                    C_muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = C_electricMuzzleMaterial;
                    break;
                case GunModule.BulletEffect.Vampire:
                    C_muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = C_vampireMuzzleMaterial;
                    break;
            }
        }

        private void ChangeLightColour(Color color)
        {
            Vector3 normalizedColor = new Vector3(color.r, color.g, color.b).normalized;
            C_light.color = new Color(normalizedColor.x, normalizedColor.y, normalizedColor.z, 1) * C_light.intensity;
        }

        public void SpawnBulletShells()
        {
            if (C_bulletShells == null)
            {
                return;
            }

            GameObject newShell = Instantiate(C_bulletShells, C_bulletShellSpawn.position, Quaternion.identity);
            newShell.transform.position += (C_gunHolder.transform.right / 10.0f);
            newShell.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            newShell.layer = 6;
            Rigidbody rigidbody = newShell.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddForce(C_gunHolder.transform.right * Random.Range(0.2f, 0.8f), ForceMode.Impulse);
                rigidbody.AddForce(C_gunHolder.transform.up * Random.Range(1.0f, 3.0f), ForceMode.Impulse);
                rigidbody.AddForce(-C_gunHolder.transform.forward * Random.Range(2.0f, 4.0f), ForceMode.Impulse);
                rigidbody.AddTorque(new Vector3(1, 0.8f, 0) * Random.Range(1.0f, 6.0f), ForceMode.Impulse);
            }
            AudioManager.PlayFmodEvent("SFX/ShellDrop", newShell.transform.position);
            Destroy(newShell, 4);
        }

        public void UpdateUI()
        {
            InGameUI.gameUI.SetMaxAmmo(i_clipSize);
            InGameUI.gameUI.SetCurrentAmmo(i_currentAmmo);
            InGameUI.gameUI.UpdateAmmoText();
        }
        //reload all at once
        private IEnumerator ReloadAfterTime()
        {
            if (b_reloading)
            {
                yield break;
            }
            if (C_gunHolder.CompareTag("Player"))
            {
                InGameUI.gameUI.ReloadBulletUI();
            }
            b_reloading = true;
            yield return new WaitForSeconds(f_reloadSpeed / 2.0f);
            if (C_gunHolder.CompareTag("Player"))
            {
                AudioManager.PlayFmodEvent("SFX/PlayerReload", transform.position);
            }
            yield return new WaitForSeconds(f_reloadSpeed / 2.0f);
            HardReload();
            b_reloading = false;
            f_timeUntilNextFire = f_timeBetweenBulletShots * 2.0f;
        }

        private IEnumerator TurnOffLight()
        {
            yield return new WaitForSeconds(f_lightOffTime);
            C_light.gameObject.SetActive(false);
        }

    }
}
