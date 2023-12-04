using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Managers;

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

        public void CreatePool(int bulletCount, GameObject bulletPrefab)
        {
            //int shotCount = gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount == 0 ? 1 : gun.aC_moduleArray[2].S_shotPatternInformation.i_shotCount;
            //float bulletTravel = gun.aC_moduleArray[0].f_bulletSpeed;
            //int bulletAmount = (int)((shotCount * gun.aC_moduleArray[1].i_clipSize * (gun.aC_moduleArray[0].f_fireRate)) * 1.4f);

            i_totalBullets = bulletCount;


            for (int i = 0; i < i_totalBullets; i++)
            {
                GameObject obj = Instantiate(bulletPrefab);
                obj.transform.parent = transform;
                obj.name = $"Bullet: {i + 1}";
                obj.layer = 6;
                Bullet bulletRef = obj.GetComponent<Bullet>();
                bulletRef.C_poolOwner = this;
                lC_freeBullets.Add(bulletRef);
                lC_allBullets.Add(bulletRef);

                obj.SetActive(false);
            }
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
    }
}
