using Utility;
using UnityEngine;

namespace Enemy
{
    public class Mite : EnemyBase
    {
        [Header("Mite Specific Variables:")]
        [Rename("Chase Behaviour")] public bool b_chasePlayer = false;
        [Rename("Look At Player")] public bool b_lookAtPlayer = false;
        [Rename("Chase Distance")] public float f_chaseDistance = 3.5f;
        [Rename("Stop Chase Distance")] public float f_stopChaseDistance = 4.5f;

        private void Start()
        {
            base.Start();
            if (!b_chasePlayer)
            {
                ChangeMovementDirection(new Vector2(transform.forward.x, transform.forward.z));
                SetRotationDirection(S_movementVec2Direction);
            }
        }


        private void Update()
        {
            base.Update();
            MeleeDamage();
            if (f_distanceToPlayer < f_aimRange && b_lookAtPlayer)
            {
                LookAtPlayer();
            }
            if (!b_chasePlayer)
            {
                if(Physics.SphereCast(transform.position, f_size, transform.forward, out RaycastHit hit, f_size * 2, i_bulletLayerMask))
                {
                    ReflectMovementDirection(new Vector2(hit.normal.x, hit.normal.z));
                    SetRotationDirection(S_movementVec2Direction);
                }
                return;
            }
            if (f_distanceToPlayer < f_chaseDistance)
            {
                ChangeMovementDirection(DirectionOfPlayer());
            }
            if (f_distanceToPlayer > f_stopChaseDistance)
            {
                ChangeMovementDirection(Vector2.zero);
            }
        }
    }
}
