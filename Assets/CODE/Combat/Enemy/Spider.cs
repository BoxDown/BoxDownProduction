using Utility;
using UnityEngine;

namespace Enemy
{
    public class Spider : EnemyBase
    {
        [Header("Spider Specific Variables:")]
        [Rename("Flee/Chase"), Tooltip("True To Make Spider Flee, False To Make Spider Chase")] public bool b_flee = false;
        [Rename("Flee Distance")] public float f_fleeDistance = 3.5f;
        [Rename("Stop Flee Distance")] public float f_stopFleeDistance = 4.5f;
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
            MeleeDamage();

            if (!b_flee)
            {
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
                else if(f_distanceToPlayer < f_fireRange)
                {
                    FireGun();
                }

                if (f_distanceToPlayer > f_stopChaseDistance)
                {
                    ChangeMovementDirection(Vector2.zero);
                }
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
