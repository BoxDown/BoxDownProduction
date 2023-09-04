using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utility;

namespace Managers
{

    public class InGameUI : MonoBehaviour
    {
        public static InGameUI gameUI
        {
            get;
            private set;
        }

        private int i_currentAmmo;
        private int i_maxAmmo;

        private float f_maxHealth;
        private float f_currentHealth;

        [Rename("Health Slider"), SerializeField] Slider C_healthSlider;
        [Rename("Ammo Slider"), SerializeField] Slider C_ammoSlider;

        [Rename("Ammo Text"), SerializeField] TextMeshPro C_ammoText;



        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (gameUI != null && gameUI != this)
            {
                Destroy(this);
            }
            else
            {
                gameUI = this;
            }
            C_healthSlider.wholeNumbers = false;
            C_ammoSlider.wholeNumbers = true;

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
            UpdateAmmoText();
            UpdateAmmoSlider();
        }
        public void UpdateAmmoText()
        {
            C_ammoText.text = $"{i_currentAmmo} / {i_maxAmmo}";
        }
        public void UpdateHealthSlider()
        {
            C_healthSlider.maxValue = f_maxHealth;
            C_healthSlider.value = f_currentHealth;
        }
        public void UpdateAmmoSlider()
        {
            C_ammoSlider.maxValue = i_maxAmmo;
            C_ammoSlider.value = i_currentAmmo;
        }
    }
}