using UnityEngine;
using Utility;

namespace Enemy
{
    public class Wasp : EnemyBase
    {
        [Header("Slug Specific Variables:")]
        [Rename("Aim At Player")] public bool b_aimAtPlayer = false;
        [Rename("Debug Fire")] public bool b_debugFire = false;
        [Rename("Chase Distance")] public float f_chaseDistance = 3.5f;
        [Rename("Stop Chase Distance")] public float f_stopChaseDistance = 4.5f;

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
            if (f_distanceToPlayer < f_aimRange)
            {
                LookAtPlayer();
            }

            if (f_distanceToPlayer < f_chaseDistance)
            {
                ChangeMovementDirection(DirectionOfPlayer());
                CancelGun();
                return;
            }
            else if (f_distanceToPlayer < f_fireRange)
            {
                FireGun();
            }

            if (f_distanceToPlayer > f_stopChaseDistance)
            {
                ChangeMovementDirection(Vector2.zero);
            }
            return;
        }
    }
}

