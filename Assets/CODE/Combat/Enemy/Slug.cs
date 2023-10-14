﻿using UnityEngine;
using Utility;
using Managers;

namespace Enemy
{
    public class Slug : EnemyBase
    {
        [Header("Slug Specific Variables:")]
        [Rename("Aim At Player")] public bool b_aimAtPlayer = false;

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
            if (b_spawning || !PlayerLineOfSightCheck())
            {
                CancelGun();
                return;
            }
            if (C_player.b_isDead)
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
                if (f_distanceToPlayer < f_fireRange)
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

        public override void Die()
        {
            base.Die();
            GameManager.IncrementSlugKill();
        }
    }
}
