using UnityEngine;
using Utility;

public class Destructable : MonoBehaviour
{
    [Rename("Object Health")] float f_health = 100;

    [Rename("Destructable Material")] Material C_destructionMaterial;
    [Rename("Destructable Grab Bag")] GameObject C_grabBag;

    
    private void DamageObject(float damage)
    {
        f_health -= damage;
        //art shit here
        //...

        if(f_health < 0)
        {
            Break();
        }
    }

    private void Break()
    {
        //art shit here
        //...
        Destroy(gameObject);
    }
}
