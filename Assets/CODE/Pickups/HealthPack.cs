using UnityEngine;
using Utility;
[RequireComponent(typeof(SphereCollider))]
public class HealthPack : Pickup
{
    private void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }
    [Rename("Health Recover Amount")] public float f_healthToRecover;
    public void OnInteract(Combatant combatant)
    {
        combatant.Heal(f_healthToRecover);
        Destroy(this);
    }
    private void FixedUpdate()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
        foreach(Collider c in collisions)
        {
            if (c.GetComponent<PlayerController>() != null)
            {
                c.GetComponent<PlayerController>().Heal(f_healthToRecover);
            }
        }
    }
}