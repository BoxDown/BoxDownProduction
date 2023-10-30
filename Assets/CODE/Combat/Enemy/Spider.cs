using Utility;
using UnityEngine;
using Managers;

namespace Enemy
{
    public class Spider : EnemyBase
    {
        [Header("Spider Specific Variables:")]

        [Rename("Chase Distance")] public float f_chaseDistance = 3.5f;

        private void OnValidate()
        {
            if (C_ownedGun == null)
            {
                throw new System.Exception("Spider Gun Is Null And Should Not Be");
            }
        }
        private void Start()
        {
            base.Start();
            SetMaterialUVOffset(C_ownedGun.aC_moduleArray[1].S_bulletEffectInformation.e_bulletEffect);
        }
        private void Update()
        {
            base.Update();


            //audio
            PlayAudio();

            // behaviour
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

            if (f_distanceToPlayer < f_chaseDistance)
            {
                ChangeMovementDirection(DirectionOfPlayer());
                CancelGun();
                return;
            }
            return;
        }
        protected override void Move()
        {
            base.Move();
            if (C_animator != null)
            {
                C_animator.SetFloat("Turn", C_animator.GetFloat("Strafe"));
            }
        }
        public override void Die()
        {
            base.Die();
            GameManager.IncrementSpiderKill();
        }
        public override void Damage(float damage)
        {
            base.Damage(damage);
            AudioManager.PlayFmodEvent("SFX/Enemy/Spider/Spider_Hit", transform.position);
        }
        private void PlayAudio()
        {
            if (f_currentTimeBetweenSounds < 0)
            {
                AudioManager.PlayFmodEvent("SFX/Enemy/Spider/Spider_Chatter", transform.position);
                GetNewTimeBetweenSounds();
            }
            f_currentTimeBetweenSounds -= Time.deltaTime;
        }
    }
}
