using Utility;
using UnityEngine;
using Managers;

namespace Enemy
{
    public class Spider : EnemyBase
    {
        [Header("Spider Specific Variables:")]

        [Rename("Chase Distance")] public float f_chaseDistance = 3.5f;
        [Rename("Stop Chase Distance")] public float f_stopChaseDistance = 4.5f;

        private void OnValidate()
        {
            if (C_ownedGun == null)
            {
                throw new System.Exception("Spider Gun Is Null And Should Not Be");
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
        public override void Die()
        {
            base.Die();
            GameManager.IncrementSpiderKill();
        }
    }
}
