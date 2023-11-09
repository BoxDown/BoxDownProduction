using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using TMPro;
using Gun;
using Managers;

public class GunModuleCard : MonoBehaviour
{
    [Rename("Canvas Group")] public CanvasGroup C_canvasGroup;
    [HideInInspector] public GunModule C_gunModuleReference;
    [Rename("Background Image"), SerializeField] private Image C_backgroundImage;
    [Rename("Trigger Sprite"), SerializeField] private Sprite C_triggerSprite;
    [Rename("Clip Sprite"), SerializeField] private Sprite C_clipSprite;
    [Rename("Barrel Sprite"), SerializeField] private Sprite C_barrelSprite;
    [Rename("Stat Bar Gradient")] public Gradient C_statBarGradient;
    [Rename("Gun Module Transform")] public Transform C_gunModuleTransform;
    [Rename("Gun Module Name")] public TextMeshProUGUI C_gunModuleName;
    [Rename("Module Stat 1 Name")] public TextMeshProUGUI C_moduleStat1Name;
    [Rename("Module Stat 2 Name")] public TextMeshProUGUI C_moduleStat2Name;
    [Rename("Module Stat 3 Name")] public TextMeshProUGUI C_moduleStat3Name;
    [Rename("Module Stat 4 Name")] public TextMeshProUGUI C_moduleStat4Name;

    [Rename("Stat 1 Value Bar")] public Image C_statValueBar1;
    [Rename("Stat 2 Value Bar")] public Image C_statValueBar2;
    [Rename("Stat 3 Value Bar")] public Image C_statValueBar3;
    [Rename("Stat 4 String")] public TextMeshProUGUI C_statString;

    [Rename("Card Fade Time")] public float f_fadeTime = 0.2f;

    [Rename("Swap Mesh On/Off")] public bool b_swapMeshOnOff = true;

    public void UpdateGunModule(GunModule gunModuleToSet, bool swappingModule)
    {
        Show();
        C_gunModuleReference = gunModuleToSet;
        UpdateCardInfo(swappingModule);
    }

    private void UpdateCardInfo(bool swappingModule)
    {
        C_gunModuleName.text = C_gunModuleReference.s_moduleName;
        switch (C_gunModuleReference.e_moduleType)
        {
            case GunModule.ModuleSection.Trigger:
                //Imagery
                C_backgroundImage.sprite = C_triggerSprite;
                if (b_swapMeshOnOff)
                {
                    if (!swappingModule)
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapTriggerMesh(C_gunModuleReference);
                    }
                    else
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapSwappingMesh(C_gunModuleReference);
                    }
                }
                //Stat Names
                C_moduleStat1Name.text = "Damage:";
                C_moduleStat2Name.text = "Fire Rate:";
                C_moduleStat3Name.text = "Bullet Speed:";
                C_moduleStat4Name.text = "Bullet Type:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(3.5f, 65, 0.1f, 0.9f, C_gunModuleReference.f_baseDamage), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(1.5f, 15, 0.1f, 0.9f, C_gunModuleReference.f_fireRate), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(C_statValueBar2.transform.localScale.x);
                C_statValueBar3.transform.localScale = new Vector3(ExtraMaths.Map(6.5f, 20, 0.1f, 0.9f, C_gunModuleReference.f_bulletSpeed), 1, 1);
                C_statValueBar3.color = C_statBarGradient.Evaluate(C_statValueBar3.transform.localScale.x);
                C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString();
                break;

            case GunModule.ModuleSection.Clip:
                //Imagery
                C_backgroundImage.sprite = C_clipSprite;
                if (b_swapMeshOnOff)
                {
                    if (!swappingModule)
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapClipMesh(C_gunModuleReference);
                    }
                    else
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapSwappingMesh(C_gunModuleReference);
                    }
                }
                //Stat Names
                C_moduleStat1Name.text = "Clip Size:";
                C_moduleStat2Name.text = "Reload Time:";
                C_moduleStat3Name.text = "Weight:";
                C_moduleStat4Name.text = "Bullet Element:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(32, 50, 0.1f, 0.9f, C_gunModuleReference.i_clipSize), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(1.5f, 2.5f, 0.1f, 0.9f, C_gunModuleReference.f_reloadSpeed), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(1 - C_statValueBar2.transform.localScale.x);
                C_statValueBar3.transform.localScale = new Vector3(ExtraMaths.Map(0, 2f, 0.1f, 0.9f, C_gunModuleReference.f_movementPenalty), 1, 1);
                C_statValueBar3.color = C_statBarGradient.Evaluate(1 - C_statValueBar3.transform.localScale.x);
                C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString();
                break;

            case GunModule.ModuleSection.Barrel:
                //Imagery
                C_backgroundImage.sprite = C_barrelSprite;
                if (b_swapMeshOnOff)
                {
                    if (!swappingModule)
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapBarrelMesh(C_gunModuleReference);
                    }
                    else
                    {
                        GameManager.gameManager.C_gunModuleUI.SwapSwappingMesh(C_gunModuleReference);
                    }
                }
                //Stat Names
                C_moduleStat1Name.text = "Bullet Size:";
                C_moduleStat2Name.text = "Range:";
                C_moduleStat3Name.text = "Recoil:";
                C_moduleStat4Name.text = "Shot Pattern:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(0.25f, 0.85f, 0.1f, 0.9f, C_gunModuleReference.f_bulletSize), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(2, 7, 0.1f, 0.9f, C_gunModuleReference.f_bulletRange), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(C_statValueBar2.transform.localScale.x);
                C_statValueBar3.transform.localScale = new Vector3(ExtraMaths.Map(0, 3.5f, 0.1f, 0.9f, C_gunModuleReference.f_recoil), 1, 1);
                C_statValueBar3.color = C_statBarGradient.Evaluate(1 - C_statValueBar3.transform.localScale.x);
                C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString();
                break;
        }
    }

    public void Show()
    {
        C_canvasGroup.alpha = 1;
    }

    public void Fade()
    {
        StartCoroutine(FadeOverSeconds(f_fadeTime));
    }

    private IEnumerator FadeOverSeconds(float seconds)
    {
        float startTime = Time.time;
        while ((Time.time - startTime) < seconds)
        {
            C_canvasGroup.alpha = Mathf.Lerp(1, 0, ((Time.time - startTime) / seconds));
            yield return 0;
        }
        C_canvasGroup.alpha = 0;
        yield return null;
    }
}
