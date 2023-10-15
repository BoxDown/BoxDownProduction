using UnityEngine;
using Utility;
using Managers;

public class Destructable : MonoBehaviour
{
    [Rename("Object Health")] public float f_health = 100;
    float f_currentHealth;
    [Rename("Explosive")] public bool b_explosive = false;
    [Rename("Explosion Effect")] public GameObject C_explosionEffect = null;
    [Rename("Drop Health Chance"), Range(0,100)] public float f_healthDropPercent = 0;
    [Rename("Health Pack Prefab"), Tooltip("Small/Large"), SerializeField] private GameObject C_healthPrefab;

    [Rename("Destructable Material")] Material C_destructionMaterial;
    [Rename("Destructable Grab Bag")] GameObject C_grabBag;

    private void Start()
    {
        f_currentHealth = f_health;
    }

    public void DamageObject(float damage)
    {
        f_currentHealth -= damage;
        //art shit here
        //...

        if(f_currentHealth < 0)
        {
            Break();
        }
    }

    private void Break()
    {
        //art shit here
        //...
        Destroy(gameObject);
        if (b_explosive && C_explosionEffect)
        {
            Explosion.ExplosionGenerator.MakeExplosion(transform.position, C_explosionEffect, 1.5f, 10, 2, 0.3f);
        }
        GameManager.IncrementEnvironmentDestroyed();
        if(f_healthDropPercent == 0)
        {
            return;
        }
        float randomNumber = Random.Range(0, 100);
        if(randomNumber > f_healthDropPercent)
        {
            Instantiate(C_healthPrefab);
        }
    }
}
