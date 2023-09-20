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
    }
}