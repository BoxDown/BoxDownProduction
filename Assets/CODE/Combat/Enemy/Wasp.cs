using UnityEngine;
using Utility;
using Managers;

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
        private void Update()
        {
            base.Update();
            if (b_spawning || !PlayerLineOfSightCheck())
            {
                CancelGun();
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
        }
    }
}

