using UnityEngine;
using Utility;

namespace Enemy
{

    public class EnemyBase : Combatant
    {
        [Header("Base Enemy Variables")]
        [Rename("Lock Enemy Position")] public bool b_lockEnemyPosition;
        [Rename("Aim Range")] public float f_aimRange = 6;
        [Rename("Fire Range")] public float f_fireRange = 4;
        [Rename("Melee Damage")] public float f_meleeDamage = 8;
        [Rename("Melee Knockback")]

        //runtime variables
        private PlayerController C_player;

        private void Awake()
        {
            base.Start();
            C_player = FindObjectOfType<PlayerController>();
            SetRotationDirection(new Vector2(transform.forward.x, transform.forward.z));
        }

        private void Update()
        {
            base.Update();
        }

        public void LookAtPlayer()
        {
            if (C_player != null)
            {
                Vector3 fromToPlayer = C_player.transform.position - transform.position;
                SetRotationDirection(new Vector2(fromToPlayer.x, fromToPlayer.z));
            }
        }
        public float f_distanceToPlayer
        {
            get
            {
                return (C_player.transform.position - transform.position).magnitude;
            }
        }
        public Vector2 DirectionOfPlayer()
        {
            Vector3 fromToPlayer = C_player.transform.position - transform.position;
            return new Vector2(fromToPlayer.x, fromToPlayer.z);
        }

        public void ReflectMovementDirection(Vector2 normal)
        {
            ChangeMovementDirection(Vector2.Reflect(S_movementVec2Direction, normal));
        }

        protected override void Move()
        {
            base.Move();
        }
        private void OnCollisionEnter(Collision collision)
        {
            PlayerController player = collision.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(f_meleeDamage);
            }
        }
    }
}