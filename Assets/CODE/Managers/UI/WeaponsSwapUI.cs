using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Gun;
using UnityEngine.InputSystem;

namespace Managers
{

    public class WeaponsSwapUI : MonoBehaviour
    {
        static public WeaponsSwapUI swapUI
        {
            get;
            private set;
        }

        [Rename("Trigger Module Card"), SerializeField] private GunModuleCard C_triggerCard;
        [Rename("Clip Module Card"), SerializeField] private GunModuleCard C_clipCard;
        [Rename("Barrel Module Card"), SerializeField] private GunModuleCard C_barrelCard;
        [Rename("Swap Module Card"), SerializeField] private GunModuleCard C_swapCard;

        // magic numbers -700 -225 255 700
        Vector2 S_left = new Vector2(-700,-150);
        Vector2 S_middleLeft = new Vector2(-255, 0);
        Vector2 S_middleRight = new Vector2(255, 0);
        Vector2 S_right = new Vector2(700, -150);



        Transform C_swappingModuleTransform;
        GunModule C_swappingModule;

        private void Awake()
        {
            if (swapUI != null && swapUI != this)
            {
                Destroy(this);
            }
            else
            {
                swapUI = this;
            }
        }
        public static void Activate(GunModule gunModule, Transform swappingModule)
        {
            GameManager.gameManager.SetCulling(false);
            GameManager.SwitchToSwapUIActions();
            switch (gunModule.e_moduleType)
            {
                case GunModule.ModuleSection.Trigger:
                    swapUI.ReadySwapTrigger();
                    break;
                case GunModule.ModuleSection.Clip:
                    swapUI.ReadySwapClip();
                    break;
                case GunModule.ModuleSection.Barrel:
                    swapUI.ReadySwapBarrel();
                    break;
            }
            swapUI.UpdateGunModels(gunModule);
            swapUI.C_swappingModuleTransform = swappingModule;
            swapUI.C_swappingModule = gunModule;
            swapUI.gameObject.SetActive(true);
            GameManager.CurrentSelectionSwapMenu();
        }

        public static void SwapAction(InputAction.CallbackContext context)
        {
            swapUI.ConfirmSwap();
        }
        public static void BackAction(InputAction.CallbackContext context)
        {
            swapUI.TurnOff();
        }

        public static void Deactivate()
        {
            swapUI.gameObject.SetActive(false);
            GameManager.gameManager.SetCulling(true);
        }

        public void TurnOff()
        {
            GameManager.SwitchToInGameActions();
            swapUI.C_swappingModuleTransform = null;
            swapUI.C_swappingModule = null;
            Deactivate();
        }

        private void ReadySwapTrigger()
        {
            GameManager.gameManager.C_gunModuleUI.PlayTriggerIdle();
            //Set Positions
            C_clipCard.transform.localPosition = S_left;
            C_triggerCard.transform.localPosition = S_middleLeft;
            C_swapCard.transform.localPosition = S_middleRight;
            C_barrelCard.transform.localPosition = S_right;
        }
        private void ReadySwapClip()
        {
            GameManager.gameManager.C_gunModuleUI.PlayClipIdle();
            //Set Positions
            C_triggerCard.transform.localPosition = S_left;
            C_clipCard.transform.localPosition = S_middleLeft;
            C_swapCard.transform.localPosition = S_middleRight;
            C_barrelCard.transform.localPosition = S_right;
        }
        private void ReadySwapBarrel()
        {
            GameManager.gameManager.C_gunModuleUI.PlayBarrelIdle();
            //Set Positions
            C_triggerCard.transform.localPosition = S_left;
            C_barrelCard.transform.localPosition = S_middleLeft;
            C_swapCard.transform.localPosition = S_middleRight;
            C_clipCard.transform.localPosition = S_right;
        }
        private void UpdateGunModels(GunModule swappingModule)
        {
            swapUI.C_triggerCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[0],false);
            swapUI.C_clipCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[1],false);
            swapUI.C_barrelCard.UpdateGunModule(GameManager.GetPlayer().C_ownedGun.aC_moduleArray[2],false);
            swapUI.C_swapCard.UpdateGunModule(swappingModule,true);
        }
        public void ConfirmSwap()
        {
            StartCoroutine(WeaponSwapRoutine());
        }

        private IEnumerator WeaponSwapRoutine()
        {
            GameManager.GetPlayer().SwapModule(C_swappingModuleTransform);

            C_triggerCard.Fade();
            C_clipCard.Fade();
            C_barrelCard.Fade();
            C_swapCard.Fade();

            yield return new WaitForSeconds(C_clipCard.f_fadeTime);
            switch (swapUI.C_swappingModule.e_moduleType)
            {
                case GunModule.ModuleSection.Trigger:
                    GameManager.gameManager.C_gunModuleUI.PlayTriggerSwap();
                    break;
                case GunModule.ModuleSection.Clip:
                    GameManager.gameManager.C_gunModuleUI.PlayClipSwap();
                    break;
                case GunModule.ModuleSection.Barrel:
                    GameManager.gameManager.C_gunModuleUI.PlayBarrelSwap();
                    break;
            }
            yield return new WaitForSeconds(1.3f);
            AudioManager.PlayFmodEvent("SFX/Player/Gun_Change", GameManager.GetCamera().transform.position);

            yield return new WaitForSeconds(0.3f);
            TurnOff();
        }
    }

}