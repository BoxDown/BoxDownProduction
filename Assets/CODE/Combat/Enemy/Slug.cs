using UnityEngine;
using Utility;
using Managers;
using System.Collections;

namespace Enemy
{
    public class Slug : EnemyBase
    {
        [Header("Slug Specific Variables:")]
        [Rename("Aim At Player")] public bool b_aimAtPlayer = false;
        [Rename("Snap Aim On Hit")] public bool b_aimAfterhit = false;

        private void Start()
        {
            base.Start();
            SetMaterialUVOffset(C_ownedGun.aC_moduleArray[1].S_bulletEffectInformation.e_bulletEffect);
        }
        protected override IEnumerator SpawnRoutine()
        {
            AudioManager.PlayFmodEvent("SFX/SlugSpawn", transform.position);
            StartCoroutine(base.SpawnRoutine());
            yield return null;
        }

        private void OnValidate()
        {
            if (C_ownedGun == null)
            {
                throw new System.Exception("Slug Gun Is Null And Should Not Be");
            }
        }

        protected override void Move()
        {
            return;
        }

        private void Update()
        {
            base.Update();

            // behaviour

            if (b_spawning || C_player.b_isDead || e_combatState == CombatState.Frozen || b_isDead)
            {
                CancelGun();
                return;
            }
            MeleeDamage();
            if (b_aimAtPlayer)
            {
                if (f_distanceToPlayer < f_aimRange)
                {
                    LookAtPlayer();
                }
                if (f_distanceToPlayer < f_fireRange && PlayerLineOfSightCheck())
                {
                    FireGun(); 
                }
                else
                {
                    CancelGun();
                }
            }
            else
            {
                FireGun();
                EyeballLookAtPlayer();
            }
        }
        public override void Damage(float damage)
        {
            base.Damage(damage);
            if (b_aimAfterhit && !b_isDead)
            {
                SetRotationDirection(Vector2.ClampMagnitude(DirectionOfPlayer(), 0.1f));
            }
            AudioManager.PlayFmodEvent("SFX/EnemyHit", transform.position);
        }

        public override void Die()
        {
            base.Die();
            GameManager.IncrementSlugKill();
            AudioManager.PlayFmodEvent("SFX/LargeEnemyDeath", transform.position);
        }
    }
}
