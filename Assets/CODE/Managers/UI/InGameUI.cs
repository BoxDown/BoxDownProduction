using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utility;
using Gun;
using System.Linq;

namespace Managers
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI gameUI
        {
            get;
            private set;
        }

        [Rename("Health Colour Gradient"), SerializeField] private Gradient C_healthGradient;

        private int i_currentAmmo = 1;
        private int i_maxAmmo = 1;

        private float f_maxHealth = 1;
        private float f_currentHealth = 1;

        [Header("Health")]
        [Rename("Health Slider"), SerializeField] Image C_healthSlider;
        [Space(10)]

        [Header("Bullet UI Variables")]
        [Rename("Bullet Start Position"), SerializeField] Transform C_bulletStartTransform;
        [Rename("Bullet Image "), SerializeField] Image C_ammoImage;
        [Rename("Distance Between Bullets")]
        List<Image> lC_bulletUIPool = null;
        int i_currentBullet = 0;
        [Rename("Ammo Animation Curve Fire"), SerializeField] AnimationCurve C_bulletAnimationCurveFire;
        [Rename("Ammo Animation Curve Reload"), SerializeField] AnimationCurve C_bulletAnimationCurveReload;
        [Rename("Ammo Text"), SerializeField] TextMeshProUGUI C_ammoText;
        [Space(10)]

        [Header("Gun Module Card")]
        [Rename("Gun Module Card"), SerializeField] GunModuleCard C_gunModuleCard;
        [Rename("Single Module Transform"), SerializeField] Transform C_gunModuleTransform;

        [Header("Stat Texts")]
        [Rename("Room Count Text"), SerializeField] TextMeshProUGUI C_roomCountText;
        [Rename("Room Count Group"), SerializeField] CanvasGroup C_roomCountGroup;
        [Rename("Enemy Count Text"), SerializeField] TextMeshProUGUI C_enemyCountText;
        [Rename("Enemy Count Group"), SerializeField] CanvasGroup C_enemyCountGroup;

        [Header("Interactable UI")]
        [Rename("Interaction Group"), SerializeField] Transform C_interactionGroup;
        [Rename("Interaction Button"), SerializeField] Image C_interactionButtonImage;
        [Rename("Interaction Text"), SerializeField] TextMeshProUGUI C_interactionText;
        private Transform C_closestInteractable;
        private Transform C_closestInteractablelastFrame;
        

        private string s_currentActiveModuleName;
        private bool b_bulletsReloaded = false;



        // Start is called before the first frame update
        void Awake()
        {
            if (gameUI != null && gameUI != this)
            {
                Destroy(this);
            }
            else
            {
                gameUI = this;
            }
        }

        private void FixedUpdate()
        {
            if (C_closestInteractable == null)
            {
                TurnOffButtonPrompt();
                TurnOffGunModuleCard();
                C_closestInteractablelastFrame = C_closestInteractable;
                return;
            }
            else if (!C_interactionGroup.gameObject.activeInHierarchy)
            {
                TurnOnButtonPrompt();
            }
            UpdateButtonPrompt();
            C_closestInteractablelastFrame = C_closestInteractable;
        }

        public static void ActivateInGameUI()
        {
            gameUI.gameObject.SetActive(true);
        }
        public static void DeactivateInGameUI()
        {
            gameUI.gameObject.SetActive(false);
        }

        public void SetMaxHealth(float maxHealth)
        {
            f_maxHealth = maxHealth;
        }

        public void SetCurrentHealth(float health)
        {
            f_currentHealth = health;
            UpdateHealthSlider();
        }
        public void SetMaxAmmo(int maxAmmo)
        {
            i_maxAmmo = maxAmmo;
        }
        public void SetCurrentAmmo(int ammo)
        {
            i_currentAmmo = ammo;
        }
        public void UpdateAmmoText()
        {
            C_ammoText.text = $"{i_currentAmmo} / {i_maxAmmo}";
        }
        public void UpdateHealthSlider()
        {
            float percentageHealth = f_currentHealth / f_maxHealth;
            C_healthSlider.rectTransform.localScale = new Vector3(percentageHealth, 1, 1);
            C_healthSlider.color = C_healthGradient.Evaluate(percentageHealth);
        }

        public void CreateUIBulletPool(int newBulletCount)
        {
            if (lC_bulletUIPool != null)
            {
                for (int i = 0; i < lC_bulletUIPool.Count; i++)
                {
                    Destroy(lC_bulletUIPool[lC_bulletUIPool.Count - 1 - i].gameObject);
                }
                lC_bulletUIPool = null;
            }
            lC_bulletUIPool = new List<Image>();
            for (int i = 0; i < newBulletCount; i++)
            {
                GameObject ammoPiece = Instantiate(C_ammoImage, C_bulletStartTransform.transform).gameObject;
                ammoPiece.transform.localPosition = new Vector3(0, ((-i * 10)), 0);
                lC_bulletUIPool.Add(ammoPiece.GetComponent<Image>());
            }
            i_currentBullet = lC_bulletUIPool.Count;
        }


        //TO DO FIX THIS ASAP
        public void UpdateBulletCount(int newBulletCount)
        {
            HardReloadUI();
            int difference = newBulletCount - i_currentBullet;

            //if the difference is 0 do nothing
            if(difference == 0)
            {
                return;
            }

            //if it is greater than 0, instantiate the difference and add it to the list
            if(difference > 0)
            {
                for (int i = 0; i < difference; i++)
                {
                    GameObject ammoPiece = Instantiate(C_ammoImage, C_bulletStartTransform.transform).gameObject;
                    ammoPiece.transform.localPosition = new Vector3(0, (((-i_currentBullet - i) * 10)), 0);
                    lC_bulletUIPool.Add(ammoPiece.GetComponent<Image>());
                }
            }
            //if it is less than 0, destroy the image, then remove it from the end of the list
            else
            {
                for (int i = 0; i < -difference; i++)
                {
                    Destroy(lC_bulletUIPool[lC_bulletUIPool.Count - 1].gameObject);
                    lC_bulletUIPool.RemoveAt(lC_bulletUIPool.Count - 1);
                }
            }

            i_currentBullet = lC_bulletUIPool.Count;

        }

        public void BulletFireUI()
        {
            StartCoroutine(FireBulletCoroutine(lC_bulletUIPool[i_currentBullet - 1].rectTransform));
        }

        public void ReloadBulletUI()
        {
            StartCoroutine(ReloadBulletCoroutine());
        }

        public void HardReloadUI()
        {
            int currentBullet = i_currentBullet;

            //set them to their original position
            List<Vector3> originalPositions = new List<Vector3>();
            for (int i = 0; i < lC_bulletUIPool.Count - currentBullet; i++)
            {
                originalPositions.Add(lC_bulletUIPool[lC_bulletUIPool.Count - i - 1].transform.position);
            }
            List<Vector3> goalPositions = new List<Vector3>();
            for (int i = 0; i < originalPositions.Count; i++)
            {
                goalPositions.Add(originalPositions[i] + new Vector3(-150, 0, 0));
            }
            for (int i = 0; i < originalPositions.Count; i++)
            {
                lC_bulletUIPool[lC_bulletUIPool.Count - i - 1].transform.position = goalPositions[i];
            }

            //then, get the difference between the two
            i_currentBullet = lC_bulletUIPool.Count;
        }

        public static void TurnOnGunModuleCard(GunModule module)
        {
            if (gameUI.s_currentActiveModuleName == module.name)
            {
                return;
            }
            gameUI.C_gunModuleCard.gameObject.SetActive(true);
            gameUI.C_gunModuleCard.UpdateGunModule(module, false);

            Vector3 modulePosition = gameUI.C_gunModuleTransform.position;

            Destroy(gameUI.C_gunModuleTransform.gameObject);
            gameUI.C_gunModuleTransform = Instantiate(module.C_meshPrefab, gameUI.transform).transform;
            Destroy(gameUI.C_gunModuleTransform.Find("ModuleEffects").gameObject);

            gameUI.C_gunModuleTransform.position = modulePosition;
            if (gameUI.C_gunModuleTransform.GetComponent<Collider>() != null)
            {
                Destroy(gameUI.C_gunModuleTransform.GetComponent<Collider>());
            }
            foreach (Transform child in gameUI.C_gunModuleTransform)
            {
                child.gameObject.layer = 8;
            }
            gameUI.s_currentActiveModuleName = module.name;
        }
        public static void TurnOffGunModuleCard()
        {
            gameUI.C_gunModuleCard.gameObject.SetActive(false);
            gameUI.s_currentActiveModuleName = "";
        }

        public IEnumerator FireBulletCoroutine(RectTransform bulletToFire)
        {
            if (b_bulletsReloaded)
            {
                yield break;
            }
            Vector3 originalPosition = bulletToFire.position;
            Vector3 goalPosition = originalPosition + new Vector3(150, 0, 0);
            float startTime = Time.time;
            i_currentBullet -= 1;
            while (Time.time - startTime < 0.15f)
            {
                yield return 0;
                if (b_bulletsReloaded)
                {
                    bulletToFire.position = goalPosition;
                    yield break;
                }
                bulletToFire.position = Vector3.Lerp(originalPosition, goalPosition, C_bulletAnimationCurveFire.Evaluate((Time.time - startTime) / 0.15f));
            }
            bulletToFire.position = goalPosition;
        }
        public IEnumerator ReloadBulletCoroutine()
        {
            b_bulletsReloaded = true;
            yield return 0;
            List<Vector3> originalPositions = new List<Vector3>();
            for (int i = 0; i < lC_bulletUIPool.Count - i_currentBullet; i++)
            {
                originalPositions.Add(lC_bulletUIPool[lC_bulletUIPool.Count - i - 1].transform.position);
            }
            List<Vector3> goalPositions = new List<Vector3>();
            for (int i = 0; i < originalPositions.Count; i++)
            {
                goalPositions.Add(originalPositions[i] + new Vector3(-150, 0, 0));
            }
            float reloadTime = GameManager.GetPlayer().C_ownedGun.aC_moduleArray[1].f_reloadSpeed;
            float startTime = Time.time;
            while (Time.time - startTime < reloadTime)
            {
                yield return 0;
                for (int i = 0; i < originalPositions.Count; i++)
                {
                    Vector3 positionToSet = Vector3.Lerp(originalPositions[i], goalPositions[i], C_bulletAnimationCurveReload.Evaluate((Time.time - startTime) / reloadTime));
                    lC_bulletUIPool[lC_bulletUIPool.Count - i - 1].transform.position = positionToSet;
                }
            }
            i_currentBullet = lC_bulletUIPool.Count;
            b_bulletsReloaded = false;
        }

        #region RoomEnemyInformation
        public static void FadeInRoomCount()
        {
            gameUI.StartCoroutine(gameUI.FadeRoomCountIn());
        }
        public static void FadeOutRoomCount()
        {
            gameUI.StartCoroutine(gameUI.FadeRoomCountOut());
        }
        public static void FadeInEnemyCount()
        {
            gameUI.StartCoroutine(gameUI.FadeEnemyCountIn());
        }
        public static void FadeOutEnemyCount()
        {
            gameUI.StartCoroutine(gameUI.FadeEnemyCountOut());
        }


        public static void UpdateEnemyCountText(float enemyCount, bool finalWave, int waveNumber)
        {
            if (finalWave)
            {
                gameUI.C_enemyCountText.text = $"Final Wave! \nEnemies Remaining: \n {enemyCount}";
                return;
            }
            gameUI.C_enemyCountText.text = $"Waves Remaining: {waveNumber}! \nEnemies Remaining: \n {enemyCount}";
        }
        public static void UpdateRoomCountText()
        {
            gameUI.C_roomCountText.text = $"Rooms Cleared: {GameManager.GetRoomsCleared()}";
        }

        #region FadeCoroutines

        private IEnumerator FadeRoomCountIn()
        {
            float time = 0;
            while (time < 0.5f)
            {
                C_roomCountGroup.alpha = Mathf.Lerp(0, 1, time / 0.5f);
                yield return 0;
                time += Time.deltaTime;
            }
            C_roomCountGroup.alpha = 1;
        }
        private IEnumerator FadeRoomCountOut()
        {
            float time = 0;
            while (time < 2)
            {
                C_roomCountGroup.alpha = Mathf.Lerp(1, 0, time / 2);
                yield return 0;
                time += Time.deltaTime;
            }
            C_roomCountGroup.alpha = 0;
        }
        private IEnumerator FadeEnemyCountIn()
        {
            float time = 0;
            while (time < 0.5f)
            {
                C_enemyCountGroup.alpha = Mathf.Lerp(0, 1, time / 0.5f);
                yield return 0;
                time += Time.deltaTime;
            }
            C_enemyCountGroup.alpha = 1;
        }
        private IEnumerator FadeEnemyCountOut()
        {
            float time = 0;
            while (time < 2)
            {
                C_enemyCountGroup.alpha = Mathf.Lerp(1, 0, time / 2);
                yield return 0;
                time += Time.deltaTime;
            }
            C_enemyCountGroup.alpha = 0;
        }

        #endregion
        #endregion

        public static void SetClosestInteractable(Transform newTransform)
        {
            gameUI.C_closestInteractable = newTransform;
        }


        public static void TurnOnButtonPrompt()
        {
            if (gameUI.C_interactionButtonImage.gameObject.activeInHierarchy)
            {
                return;
            }
            gameUI.C_interactionGroup.gameObject.SetActive(true);
        }

        public void UpdateButtonPrompt()
        {
            if(C_closestInteractable != null)
            {
                Vector3 targetPosition = Camera.main.WorldToScreenPoint(C_closestInteractable.GetComponent<Collider>().ClosestPoint(GameManager.GetPlayer().transform.position) - Vector3.up);
                gameUI.C_interactionGroup.transform.position = Vector3.Lerp(gameUI.C_interactionGroup.transform.position, targetPosition, Vector3.Distance(gameUI.C_interactionGroup.transform.position, targetPosition));
                if(C_closestInteractablelastFrame != C_closestInteractable)
                {
                    if (C_closestInteractable.tag == "Gun Module")
                    {
                        ChangeButtonPromptText(false);
                        TurnOnGunModuleCard(GunModuleSpawner.GetGunModule(C_closestInteractable.name));
                    }
                    else if (C_closestInteractable.tag == "Door")
                    {
                        ChangeButtonPromptText(true, C_closestInteractable.GetComponent<Door>().e_roomType);
                        TurnOffGunModuleCard();
                    }
                }
            }
        }

        public static void ChangeButtonPromptText(bool isDoor, Door.RoomType roomType = Door.RoomType.None)
        {
            if (!isDoor)
            {
                gameUI.C_interactionText.text = "Swap Module";
                return;
            }
            else
            {
                if(roomType == Door.RoomType.RandomModule)
                {
                    gameUI.C_interactionText.text = $"Enter Random Room";
                    return;
                }
                gameUI.C_interactionText.text = roomType == Door.RoomType.None ? "Enter Room" : $"Enter {roomType} Room";
            }
        }

        public static void TurnOffButtonPrompt()
        {
            if (!gameUI.C_interactionButtonImage.gameObject.activeInHierarchy)
            {
                return;
            }
            gameUI.C_interactionGroup.gameObject.SetActive(false);
        }


    }
}