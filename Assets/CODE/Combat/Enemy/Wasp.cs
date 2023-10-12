using UnityEngine;
using Utility;
using Managers;

namespace Enemy
{
    public class Wasp : EnemyBase
    {
        [Header("Wasp Specific Variables:")]

        [Rename("Flee Distance")] public float f_fleeDistance = 3.5f;
        [Rename("Stop Flee Distance")] public float f_stopFleeDistance = 4.5f;

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
            if (b_spawning)
            {
                return;
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
            if (f_distanceToPlayer > f_stopFleeDistance)
            {
                ChangeMovementDirection(Vector2.zero);
            }
            if (f_distanceToPlayer < f_aimRange)
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
        }
        public override void Die()
        {
            base.Die();
            GameManager.IncrementWaspKill();
        }
    }
}

