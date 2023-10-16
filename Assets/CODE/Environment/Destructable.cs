using UnityEngine;
using Utility;
using Managers;

public class Destructable : MonoBehaviour
{
    [Header("Object Variables")]
    [Rename("Object Health")] public float f_health = 100;
    float f_currentHealth;
    [Rename("Half Destroyed Object")] public GameObject C_halfDestroyedObject;
    [Rename("Destructable Grab Bag")] public GameObject C_grabBag;
    [Space(10)]

    [Header("Explosive Variables")]
    [Rename("Explosive")] public bool b_explosive = false;
    [Rename("Explosion Damage")] public float f_damage = 10;
    [Rename("Explosion Size")] public float f_size = 1.5f;
    [Rename("Explosion Duration")] public float f_length = 1f;
    [Rename("Explosion Duration")] public float f_knockback = 1f;
    [Rename("Explosion Effect")] public GameObject C_explosionEffect = null;

    [Space(10)]
    [Header("Drop Health Variables")]
    [Rename("Drop Health Chance"), Range(0,100)] public float f_healthDropPercent = 0;
    [Rename("Health Pack Prefab"), Tooltip("Small/Large"), SerializeField] private GameObject C_healthPrefab;


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
        Destroy(gameObject);
        //Destroy(Instantiate(C_grabBag), 2.0f)
        if (b_explosive && C_explosionEffect)
        {
            Explosion.ExplosionGenerator.MakeExplosion(transform.position, C_explosionEffect, f_size, f_damage, f_knockback, f_length);
        }
        GameManager.IncrementEnvironmentDestroyed();
        if(f_healthDropPercent == 0)
        {
            return;
        }
        float randomNumber = Random.Range(0, 100);
        if(randomNumber < f_healthDropPercent)
        {
            Instantiate(C_healthPrefab, transform.position, Quaternion.identity);
        }
    }
}
