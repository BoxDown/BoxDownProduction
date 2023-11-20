using UnityEngine;
using Utility;
using Managers;

public class Destructable : MonoBehaviour
{
    [Header("Object Variables")]
    [Rename("Object Health")] public float f_health = 100;
    float f_currentHealth;
    [Rename("Mesh Transform")] public Transform C_meshTransform;
    [Rename("Half Destroyed Object")] public GameObject C_halfDestroyedObject;
    [Rename("Destructable Gut Bag")] public GameObject C_grabBag;
    [Rename("Destructable Gut Bag Offset")] public Vector3 S_grabBagOffset = Vector3.zero;
    [Space(10)]

    [Header("Explosive Variables")]
    [Rename("Explosive")] public bool b_explosive = false;
    [Rename("Explosion Damage")] public float f_damage = 10;
    [Rename("Explosion Size")] public float f_size = 1.5f;
    [Rename("Explosion Size Over Lifetime")] public AnimationCurve C_sizeOverLifetimeCurve;
    [Rename("Explosion Duration")] public float f_length = 1f;
    [Rename("Explosion Knockback")] public float f_knockback = 1f;
    [Rename("Explosion Effect")] public GameObject C_explosionEffect = null;

    [Space(10)]
    [Header("Drop Health Variables")]
    [Rename("Drop Health Chance"), Range(0,100)] public float f_healthDropPercent = 0;
    [Rename("Health Pack Prefab"), Tooltip("Small/Large"), SerializeField] private GameObject C_healthPrefab;

    private bool b_objectChanged = false;


    private void Start()
    {
        f_currentHealth = f_health;
    }

    public void DamageObject(float damage)
    {
        f_currentHealth -= damage;
        //art shit here
        //...
        if(C_halfDestroyedObject != null && f_currentHealth < f_health && !b_objectChanged)
        {
            SwapMesh();
        }

        if(f_currentHealth < 0)
        {
            Break();
        }
    }

    private void Break()
    {
        AudioManager.PlayFmodEvent("SFX/Environment/Box_Break", transform.position);
        Destroy(gameObject);
        SpawnGutBag();
        if (b_explosive && C_explosionEffect)
        {
            Explosion.ExplosionGenerator.MakeExplosion(transform.position, C_explosionEffect, f_size, f_damage, f_knockback, f_length, C_sizeOverLifetimeCurve);
            FindObjectOfType<CameraDolly>().ExplosionCameraShake();
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

    private void SwapMesh()
    {
        Destroy(C_meshTransform.gameObject);

        C_meshTransform = Instantiate(C_halfDestroyedObject).transform;

        C_meshTransform.parent = transform;
        C_meshTransform.localPosition = Vector3.zero;
        C_meshTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        C_meshTransform.localScale = Vector3.one;
    }

    private void SpawnGutBag()
    {
        if (C_grabBag != null)
        {
            Destroy(Instantiate(C_grabBag, transform.position + S_grabBagOffset, C_grabBag.transform.rotation), 2.0f);
        }
    }
}
