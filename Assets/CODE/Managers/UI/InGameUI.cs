using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utility;
using Gun;

namespace Managers
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI gameUI
        {
            get;
            private set;
        }

        [Rename("Health Colour Gradient"),SerializeField] private Gradient C_healthGradient;

        private int i_currentAmmo = 1;
        private int i_maxAmmo = 1;

        private float f_maxHealth = 1;
        private float f_currentHealth = 1;

        [Rename("Health Slider"), SerializeField] Image C_healthSlider;
        [Rename("Ammo Slider"), SerializeField] Image C_ammoSlider;
        [Rename("Gun Module Card"), SerializeField] GunModuleCard C_gunModuleCard;

        [Rename("Ammo Text"), SerializeField] TextMeshProUGUI C_ammoText;
        [Rename("Reload Text"), SerializeField] TextMeshProUGUI C_reloadText;



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
        public void UpdateAmmoSlider()
        {
            C_ammoSlider.rectTransform.localScale = new Vector3(i_currentAmmo / (float)i_maxAmmo, 1, 1);
        }

        public void TurnOnReloadingText()
        {
            C_reloadText.gameObject.SetActive(true);
        }
        public void TurnOffReloadingText()
        {
            C_reloadText.gameObject.SetActive(false);
        }
        public static void TurnOnGunModuleCard(GunModule module)
        {
            gameUI.C_gunModuleCard.gameObject.SetActive(true);
            gameUI.C_gunModuleCard.UpdateGunModule(module, false);
        }
        public static void TurnOffGunModuleCard()
        {
            gameUI.C_gunModuleCard.gameObject.SetActive(false);
        }
    }
}