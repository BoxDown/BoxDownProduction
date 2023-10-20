using UnityEngine;
using Utility;
using Managers;
using System;
using System.Runtime.CompilerServices;

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

            //audio
            PlayAudio();

            // behaviour

            if (b_spawning || C_player.b_isDead)
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
            }
        }
        public override void Damage(float damage)
        {
            base.Damage(damage);
            if (b_aimAfterhit && !b_isDead)
            {
                SetRotationDirection(Vector2.ClampMagnitude(DirectionOfPlayer(), 0.1f));
            }
            AudioManager.PlayFmodEvent("SFX/Enemy/Worm/Worm_Hit", transform.position);
        }

        public override void Die()
        {
            base.Die();
            GameManager.IncrementSlugKill();
        }
        private void PlayAudio()
        {
            if (f_currentTimeBetweenSounds < 0)
            {
                AudioManager.PlayFmodEvent("SFX/Enemy/Worm/Worm_Chatter", transform.position);
                GetNewTimeBetweenSounds();
            }
            f_currentTimeBetweenSounds -= Time.deltaTime;
        }
    }
}
