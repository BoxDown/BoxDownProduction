using UnityEngine;
using Utility;

namespace Enemy
{

    public class EnemyBase : Combatant
    {

        [Rename("Lock Enemy Position")] public bool b_lockEnemyPosition;
        [Rename("Enemy Fire Testing")] public bool b_testFiring;


        //runtime variables
        private PlayerController C_player;

        private void Awake()
        {
            base.Start();
            C_player = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            base.Update();
            if(C_player != null)
            {
                Vector3 fromToPlayer =  C_player.transform.position - transform.position;
                SetRotationDirection(new Vector2(fromToPlayer.x, fromToPlayer.z));

                if (fromToPlayer.magnitude < C_ownedGun.aC_moduleArray[2].f_bulletRange)
                {
                    FireGun();
                }
                else
                {
                    CancelGun();
                }
            }            
        }

        protected override void Move()
        {
            base.Move();            
        }
    }
}