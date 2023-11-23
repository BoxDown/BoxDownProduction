using Gun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Utility;
using Managers;

namespace Explosion
{

    public class Explosion : MonoBehaviour
    {
        public GunModule C_gunModuleCreator;
        public float f_explosionSize;
        public float f_explosionDamage;
        public float f_explosionKnockbackStrength;
        public float f_explosionLifeTime;
        public AnimationCurve C_sizeOverLifeTimeCurve;
        private float f_lifeTime;
        bool b_lifeTimeReached = false;
        List<Transform> lC_alreadyCollided = new List<Transform>();
        VisualEffect C_explosionEffect;


        public void InitialiseExplosion()
        {
            f_lifeTime = 0;
            C_explosionEffect = GetComponentInChildren<VisualEffect>();
            if(C_explosionEffect != null)
            {
                C_explosionEffect.Play();
                C_explosionEffect.playRate = 1 / f_explosionLifeTime;
            }
            GameManager.IncrementExplosionCount();

            AudioManager.PlayFmodEvent("SFX/Explosion", transform.position);

        }

        private void Update()
        {
            if (LifeTimeCheck() || b_lifeTimeReached)
            {
                return;
            }
            CheckCollisions();
            transform.localScale = Vector3.one * (C_sizeOverLifeTimeCurve.Evaluate(f_lifeTime / f_explosionLifeTime) * f_explosionSize);
            f_lifeTime += Time.deltaTime;
        }

        private void CheckCollisions()
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, transform.localScale.x / 2 );
            if (collisions.Length == 0)
            {
                return;
            }
            ResolveCollisions(collisions);
        }

        private void ResolveCollisions(Collider[] collisions)
        {
            for (int i = 0; i < collisions.Length; i++)
            {
                if (lC_alreadyCollided.Contains(collisions[i].transform))
                {
                    continue;
                }

                Combatant combatant = collisions[i].transform.GetComponent<Combatant>();
                Destructable destructable = collisions[i].transform.GetComponentInParent<Destructable>();


                Vector3 hitDirection = collisions[i].transform.position - transform.position;
                hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z);
                float notCollisionDepth = Vector3.ClampMagnitude(hitDirection, 1.0f).magnitude;
                notCollisionDepth = 1 - notCollisionDepth;

                if (combatant == null && destructable == null && collisions[i].gameObject.layer != LayerMask.GetMask("Explosive"))
                {
                    lC_alreadyCollided.Add(collisions[i].transform);
                    continue;
                }
                if (combatant != null)
                {
                    if(combatant.e_combatState ==  Combatant.CombatState.Invincible || combatant.e_combatState == Combatant.CombatState.Dodge)
                    {
                        continue;
                    }
                    combatant.Damage(f_explosionDamage);
                    combatant.AddVelocity(hitDirection * (f_explosionKnockbackStrength * notCollisionDepth));
                    lC_alreadyCollided.Add(collisions[i].transform);
                    continue;
                }
                if (destructable != null)
                {
                    destructable.DamageObject(f_explosionDamage * 5.0f);
                }
            }
        }

        private bool LifeTimeCheck()
        {
            if (f_lifeTime < f_explosionLifeTime)
            {
                return false;
            }
            Destroy(gameObject, 5);
            b_lifeTimeReached = true;
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, f_explosionSize);
        }
    }

}