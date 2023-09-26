using UnityEngine;
using Utility;

namespace Enemy
{
    public class Wasp : EnemyBase
    {
        [Header("Slug Specific Variables:")]
        [Rename("Aim At Player")] public bool b_aimAtPlayer = false;
        [Rename("Debug Fire")] public bool b_debugFire = false;

        private void OnValidate()
        {
            if (C_ownedGun == null)
            {
                throw new System.Exception("Wasp Gun Is Null And Should Not Be");
            }
        }
        private void Update()
        {
            base.Update();
            MeleeDamage();
            if (C_player.b_isDead)
            {
                CancelGun();
                return;
            }
            if (!b_debugFire)
            {
                return;
            }
            if (b_aimAtPlayer && f_distanceToPlayer < f_aimRange)
            {
                LookAtPlayer();
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
    }
}

