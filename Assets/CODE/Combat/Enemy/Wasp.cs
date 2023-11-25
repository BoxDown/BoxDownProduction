using UnityEngine;
using Utility;
using Managers;
using System.Collections;

namespace Enemy
{
    public class Wasp : EnemyBase
    {
        [Header("Wasp Specific Variables:")]

        [Rename("Flee Distance")] public float f_fleeDistance = 3.5f;

        private void OnValidate()
        {
            if (C_ownedGun == null)
            {
                throw new System.Exception("Wasp Gun Is Null And Should Not Be");
            }
        }
        private void Start()
        {
            base.Start();
            SetMaterialUVOffset(C_ownedGun.aC_moduleArray[1].S_bulletEffectInformation.e_bulletEffect);

        }
        protected override IEnumerator SpawnRoutine()
        {
            AudioManager.PlayFmodEvent("SFX/WaspSpawn", transform.position);
            StartCoroutine(base.SpawnRoutine());
            yield return null;
        }
        private void Update()
        {
            base.Update();

            if(e_combatState == CombatState.Frozen || b_isDead)
            {
                CancelGun();
                return;
            }


            // behaviour
            if (b_spawning || !PlayerLineOfSightCheck())
            {
                CancelGun();
                ChangeMovementDirection(Vector2.zero);
                return;
            }
            if (f_distanceToPlayer < f_aimRange)
            {
                LookAtPlayer();
                if (f_distanceToPlayer < f_fireRange)
                {
                    ChangeMovementDirection(Vector2.zero);
                    FireGun();
                }
                else
                {
                    CancelGun();
                }
            }
            MeleeDamage();

            if (C_player.b_isDead)
            {
                CancelGun();
                return;
            }
            if (f_distanceToPlayer < f_fleeDistance)
            {
                ChangeMovementDirection(-DirectionOfPlayer());
            }

        }
        public override void Die()
        {
            base.Die();
            GameManager.IncrementWaspKill();
            AudioManager.PlayFmodEvent("SFX/SmallEnemyDeath", transform.position);

        }

        public override void Damage(float damage)
        {
            base.Damage(damage);
            AudioManager.PlayFmodEvent("SFX/EnemyHit", transform.position);
        }

    }
}

