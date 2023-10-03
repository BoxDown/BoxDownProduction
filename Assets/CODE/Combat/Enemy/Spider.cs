using Utility;
using UnityEngine;

namespace Enemy
{
    public class Spider : EnemyBase
    {
        [Header("Spider Specific Variables:")]
        [Rename("Flee Distance")] public float f_fleeDistance = 3.5f;
        [Rename("Stop Flee Distance")] public float f_stopFleeDistance = 4.5f;

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
    }
}
