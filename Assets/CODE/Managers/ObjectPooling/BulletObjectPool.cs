using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gun
{
    public class BulletObjectPool : MonoBehaviour
    {
        Material C_bulletMaterial;
        Mesh C_bulletMesh;
        List<Bullet> lC_allBullets = new List<Bullet>();
        List<Bullet> lC_freeBullets = new List<Bullet>();
        List<Bullet> lC_inUseBullets = new List<Bullet>();
        int i_totalBullets;
        public Gun C_gun = null;

        public void CreatePool(Gun gun)
        {
            C_gun = gun;
            int shotCount = gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount == 0 ? 1 : gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount;
            int bulletAmount = gun.aC_moduleArray[1].i_clipSize * shotCount * (int)gun.aC_moduleArray[0].f_fireRate;
            i_totalBullets = (int)(bulletAmount);
            C_bulletMaterial = new Material(Shader.Find("HDRP/Lit"));
            C_bulletMesh = new Mesh();
            C_bulletMesh.name = C_gun.C_gunHolder.name + ": Bullet Mesh";
            C_bulletMaterial.name = C_gun.C_gunHolder.name + ": Bullet Material";
            C_bulletMaterial.SetInt("_UseEmissiveIntensity", 1);
            C_bulletMaterial.SetInt("_EmissiveIntensityUnit", 0);
            C_bulletMaterial.SetFloat("_EmissiveIntensity", Mathf.Pow(2, C_gun.f_emissiveValue) * (4 * Mathf.PI) * 0.01f);


            for (int i = 0; i < i_totalBullets; i++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.parent = transform;
                obj.name = $"Bullet: {i + 1}";
                obj.layer = 6;
                Bullet bulletRef = obj.AddComponent<Bullet>();
                bulletRef.C_poolOwner = this;
                lC_freeBullets.Add(bulletRef);
                lC_allBullets.Add(bulletRef);

                obj.SetActive(false);
                UpdateBulletColour();
                obj.GetComponent<Renderer>().sharedMaterial = C_bulletMaterial;
                obj.GetComponent<MeshFilter>().mesh = C_bulletMesh;
            }

        }

        public void ResizePool(Gun gun)
        {
            int shotCount = gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount == 0 ? 1 : gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount;
            int bulletAmount = (int)(gun.aC_moduleArray[1].i_clipSize * shotCount * gun.aC_moduleArray[0].f_fireRate);

            int countDifference = bulletAmount - i_totalBullets;
            UpdateBulletColour();
            if (countDifference == 0)
            {
                return;
            }
            if (countDifference < 0)
            {
                for (int i = 0; i < -countDifference; i++)
                {
                    Bullet bulletToRemove = lC_allBullets[lC_allBullets.Count - 1];
                    lC_allBullets.Remove(bulletToRemove);
                    if (bulletToRemove.gameObject.activeInHierarchy)
                    {
                        lC_inUseBullets.Remove(bulletToRemove);
                        Destroy(bulletToRemove.gameObject);
                    }
                    else
                    {
                        lC_freeBullets.Remove(bulletToRemove);
                        Destroy(bulletToRemove.gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < countDifference; i++)
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.transform.parent = transform;
                    obj.name = $"Bullet: {i_totalBullets + i + 1}";
                    obj.layer = 6;
                    Bullet bulletRef = obj.AddComponent<Bullet>();
                    bulletRef.C_poolOwner = this;
                    lC_allBullets.Add(bulletRef);
                    lC_freeBullets.Add(bulletRef);
                    obj.SetActive(false);
                    obj.GetComponent<MeshFilter>().sharedMesh = C_bulletMesh;
                    obj.GetComponent<Renderer>().sharedMaterial = C_bulletMaterial;
                }
            }
            i_totalBullets = lC_allBullets.Count;
        }
        public Bullet GetFirstOpen()
        {
            return lC_freeBullets[0];
        }
        public void MoveToOpen(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            lC_freeBullets.Add(bullet);
            lC_inUseBullets.Remove(bullet);
        }
        public void MoveToClosed(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
            lC_inUseBullets.Add(bullet);
            lC_freeBullets.Remove(bullet);
        }
        public void UpdateBulletColour()
        {
            Color materialColour = Color.white;
            switch (C_gun.aC_moduleArray[1].S_bulletEffectInformation.e_bulletEffect)
            {
                case GunModule.BulletEffect.None:
                    materialColour = C_gun.S_standardColour;
                    break;
                case GunModule.BulletEffect.Fire:
                    materialColour = C_gun.S_fireColour;
                    break;
                case GunModule.BulletEffect.Ice:
                    materialColour = C_gun.S_iceColour;
                    break;
                case GunModule.BulletEffect.Lightning:
                    materialColour = C_gun.S_lightningColour;
                    break;
                case GunModule.BulletEffect.Vampire:
                    materialColour = C_gun.S_vampireColour;
                    break;
            }
            Mesh bulletMesh = null;
            switch (C_gun.aC_moduleArray[0].S_bulletTraitInformation.e_bulletTrait)
            {
                case GunModule.BulletTrait.Standard:
                    bulletMesh = C_gun.C_standardMesh;
                    break;
                case GunModule.BulletTrait.Pierce:
                    bulletMesh = C_gun.C_pierceMesh;
                    break;
                case GunModule.BulletTrait.Ricochet:
                    bulletMesh = C_gun.C_ricochetMesh;
                    break;
                case GunModule.BulletTrait.Explosive:
                    bulletMesh = C_gun.C_explosiveMesh;
                    break;
                case GunModule.BulletTrait.Homing:
                    bulletMesh = C_gun.C_homingMesh;
                    break;
            }

            C_bulletMaterial.SetColor("_EmissiveColorLDR", materialColour * Mathf.Pow(2, C_gun.f_emissiveValue) * (4 * Mathf.PI) * 0.01f);
            C_bulletMaterial.SetColor("_EmissiveColor", materialColour * Mathf.Pow(2, C_gun.f_emissiveValue) * (4 * Mathf.PI) * 0.01f);
            C_bulletMesh.vertices = bulletMesh.vertices;
            C_bulletMesh.normals = bulletMesh.normals;
            C_bulletMesh.triangles = bulletMesh.triangles;


        }
    }
}
