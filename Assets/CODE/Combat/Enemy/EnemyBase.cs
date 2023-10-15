using Explosion;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
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
        [Rename("Melee Knockback")] public float f_meleeKnockback = 3;
        [Rename("Death VFX"), SerializeField] protected GameObject C_deathEffects = null;
        [Rename("Spawn VFX"), SerializeField] protected GameObject C_spawnEffects = null;
        [Rename("Spawn Effect Delay"), SerializeField] protected float f_spawnEffectDelay = 0;
        [Rename("Spawn Effect Offset"), SerializeField] protected Vector3 f_spawnEffectOffset = Vector3.zero;

        //runtime variables
        protected PlayerController C_player;
        protected bool b_spawning;

        private void Awake()
        {
            base.Start();
            C_player = FindObjectOfType<PlayerController>();
            SetRotationDirection(new Vector2(transform.forward.x, transform.forward.z));
        }

        protected override void Update()
        {
            base.Update();
        }

        public void Spawn()
        {
            StartCoroutine(SpawnRoutine());
        }

        protected virtual IEnumerator SpawnRoutine()
        {
            b_spawning = true;
            StartCoroutine(ChangeStateForSeconds(CombatState.Invincible, 2.5f));
            if (C_spawnEffects != null)
            {
                StartCoroutine(PlaySpawnEffect(f_spawnEffectDelay));
            }
            yield return new WaitForSeconds(2.5f);
            b_spawning = false;
        }
        protected virtual IEnumerator PlaySpawnEffect(float spawnDelay)
        {
            yield return new WaitForSeconds(spawnDelay);
            Destroy(Instantiate(C_spawnEffects, transform.position + f_spawnEffectOffset, Quaternion.identity));
        }

        protected bool PlayerLineOfSightCheck()
        {
            if (Physics.Raycast(transform.position + Vector3.up, (C_player.transform.position + Vector3.up) - (transform.position + Vector3.up), out RaycastHit hit, f_distanceToPlayer, i_bulletLayerMask))
            {
                if (hit.transform == C_player.transform)
                {
                    return true;
                }
            }
            return false;
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
                if (C_player != null) { return (C_player.transform.position - transform.position).magnitude; }
                else { return 0; }
            }
        }
        public Vector2 DirectionOfPlayer()
        {
            if (C_player != null)
            {
                if (C_player.b_isDead)
                {
                    return Vector2.zero;
                }
                Vector3 fromToPlayer = C_player.transform.position - transform.position;
                return new Vector2(fromToPlayer.x, fromToPlayer.z);
            }
            else
            {
                return Vector2.zero;
            }
        }
        public override void Die()
        {
            base.Die();
            if (C_deathEffects != null)
            {
                GameObject effect = Instantiate(C_deathEffects, transform.position, transform.rotation);
                effect.transform.localScale = new Vector3(f_size * 4, f_size * 4, f_size * 4);
                effect.GetComponentInChildren<VisualEffect>().Play();
                Destroy(effect, 4f);
            }
            StartCoroutine(DeactivateAfterSeconds(4f));
        }

        public void ReflectMovementDirection(Vector2 normal)
        {
            ChangeMovementDirection(Vector2.Reflect(S_movementVec2Direction, normal).normalized);
        }

        protected override void Move()
        {
            base.Move();
        }


        protected virtual void MeleeDamage()
        {
            if (b_isDead || C_player.b_isDead)
            {
                return;
            }
            Collider[] collisions = Physics.OverlapSphere(transform.position, f_size * 1.95f);
            PlayerController player = null;
            for (int i = 0; i < collisions.Length; i++)
            {
                if (collisions[i].transform.GetComponent<PlayerController>() != null)
                {
                    player = collisions[i].transform.GetComponent<PlayerController>();
                }
            }

            if (player != null)
            {
                if (player.e_combatState != CombatState.Invincible && player.e_combatState != CombatState.Dodge)
                {
                    player.Damage(f_meleeDamage);
                    player.AddVelocity(new Vector3(DirectionOfPlayer().x, 0, DirectionOfPlayer().y) * f_meleeKnockback);
                    return;
                }
            }
        }
        private IEnumerator DeactivateAfterSeconds(float time)
        {
            if (!b_hasAnimator)
            {
                float startTime = Time.time;
                float t = 0;
                while (Time.time - startTime < time)
                {
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                    yield return 0;
                    t += Time.deltaTime;
                }
            }
            else
            {
                yield return new WaitForSeconds(time);
            }
            gameObject.SetActive(false);
        }
    }
}