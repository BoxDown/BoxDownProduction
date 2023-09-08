using UnityEngine;
using Utility;

namespace Enemy
{

    public class EnemyBase : Combatant
    {

        [Rename("Lock Enemy Position")] public bool b_lockEnemyPosition;
        [Rename("Enemy Fire Testing")] public bool b_testFiring;
        [Rename("Enemy Fire Distancfe")] public float f_attackDistance = 20;


        //runtime variables
        private PlayerController C_player;

        private void Start()
        {
            base.Start();
            //Kyles
            GameObject[] playerObject = GameObject.FindGameObjectsWithTag("Player");
            C_player = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            base.Update();
            if(C_player != null)
            {
                Vector3 fromToPlayer =  C_player.transform.position - transform.position;
                SetRotationDirection(new Vector2(fromToPlayer.x, fromToPlayer.z));

                if (b_testFiring && fromToPlayer.magnitude < f_attackDistance)
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