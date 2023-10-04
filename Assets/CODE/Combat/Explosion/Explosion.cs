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
        private float f_lifeTime;
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

            ParticleSystem smokeParticle = transform.Find("PREFAB_VFX_Smoke").GetComponent<ParticleSystem>();
            ParticleSystem shockwaveParticle = transform.Find("PREFAB_VFX_Shockwave").GetComponent<ParticleSystem>();
            //smoke start speed needs to be radius
            smokeParticle.startSpeed = f_explosionSize;
            shockwaveParticle.startSize = f_explosionSize * 1.2f;

            //shockwave start size radius * 1.2f
        }

        private void Update()
        {
            if (LifeTimeCheck())
            {
                Destroy(gameObject);
            }
            CheckCollisions();
            transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(f_explosionSize, f_explosionSize, f_explosionSize), f_lifeTime / f_explosionLifeTime);
            f_lifeTime += Time.deltaTime;
        }

        private void CheckCollisions()
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
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

                lC_alreadyCollided.Add(collisions[i].transform);
                Combatant combatant = collisions[i].transform.GetComponent<Combatant>();
                Destructable destructable = collisions[i].transform.GetComponent<Destructable>();


                Vector3 hitDirection = collisions[i].transform.position - transform.position;
                hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z);
                float notCollisionDepth = Vector3.ClampMagnitude(hitDirection, 1.0f).magnitude;
                notCollisionDepth = 1 - notCollisionDepth;

                if (combatant == null && destructable == null && collisions[i].gameObject.layer != LayerMask.GetMask("Explosive"))
                {
                    continue;
                }
                else if (combatant != null)
                {
                    if(combatant.e_combatState ==  Combatant.CombatState.Invincible || combatant.e_combatState == Combatant.CombatState.Dodge)
                    {
                        return;
                    }
                    combatant.Damage(f_explosionDamage);
                    combatant.AddVelocity(hitDirection * (f_explosionKnockbackStrength * notCollisionDepth));
                    continue;
                }
                else if (destructable != null)
                {
                    destructable.DamageObject(f_explosionDamage);
                }
            }
        }

        private bool LifeTimeCheck()
        {
            if (f_lifeTime < f_explosionLifeTime)
            {
                return false;
            }
            return true;
        }

        private void ApplyElementDamage()
        {

        }

    }

}