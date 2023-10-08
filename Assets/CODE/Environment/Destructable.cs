using UnityEngine;
using Utility;
using Managers;

public class Destructable : MonoBehaviour
{
    [Rename("Object Health")] public float f_health = 100;
    float f_currentHealth;

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
        GameManager.IncrementEnvironmentDestroyed();
    }
}
