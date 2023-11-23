using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using TMPro;
using Gun;
using Managers;
using System.Net.Http.Headers;

public class GunModuleCard : MonoBehaviour
{
    [Rename("Canvas Group")] public CanvasGroup C_canvasGroup;
    [Rename("Text Mesh Emojis")] public TMP_SpriteAsset C_spriteAsset;
    [HideInInspector] public GunModule C_gunModuleReference;

    [Header("Background Images")]
    [Rename("Background Image"), SerializeField] private Image C_backgroundImage;
    [Rename("Trigger Sprite"), SerializeField] private Sprite C_triggerSprite;
    [Rename("Clip Sprite"), SerializeField] private Sprite C_clipSprite;
    [Rename("Barrel Sprite"), SerializeField] private Sprite C_barrelSprite;

    

    [Header("Stat Names:")]
    [Rename("Module Stat 1 Name")] public TextMeshProUGUI C_moduleStat1Name;
    [Rename("Module Stat 2 Name")] public TextMeshProUGUI C_moduleStat2Name;
    [Rename("Module Stat String Name")] public TextMeshProUGUI C_moduleStat4Name;
    
    [Header("Stat Values:")]
    [Rename("Stat 1 Value Bar")] public Image C_statValueBar1;
    [Rename("Stat 2 Value Bar")] public Image C_statValueBar2;
    [Rename("Stat String Value")] public TextMeshProUGUI C_statString;

    [Header("Other Variables:")]
    [Rename("Card Fade Time")] public float f_fadeTime = 0.2f;
    [Rename("Swap Mesh On/Off")] public bool b_swapMeshOnOff = true;
    [Rename("Stat Bar Gradient")] public Gradient C_statBarGradient;
    [Rename("Gun Module Transform")] public Transform C_gunModuleTransform;
    [Rename("Gun Module Name")] public TextMeshProUGUI C_gunModuleName;

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
                C_moduleStat4Name.text = "Bullet Type:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(3f, 21f, 0.1f, 0.9f, C_gunModuleReference.f_baseDamage + C_gunModuleReference.S_bulletTraitInformation.f_explosionDamage), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(3f, 13f, 0.1f, 0.9f, C_gunModuleReference.f_fireRate), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(C_statValueBar2.transform.localScale.x);
                switch (C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait)
                {
                    case GunModule.BulletTrait.Standard:
                        C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString();
                        C_statString.color = new Color(1f, 1f, 1f);
                        break;
                    case GunModule.BulletTrait.Pierce:
                        C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString() + "<sprite index= " + 5 + ">";
                        C_statString.color = new Color(0.2f, 0.85f, 0.55f);
                        break;
                    case GunModule.BulletTrait.Ricochet:
                        C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString() + "<sprite index= " + 6 + ">";
                        C_statString.color = new Color(1f, 0.83f, 0.1f);
                        break;
                    case GunModule.BulletTrait.Explosive:
                        C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString() + "<sprite index= " + 4 + ">";
                        C_statString.color = new Color(1f, 0.62f, 0.08f);
                        break;
                    case GunModule.BulletTrait.Homing:
                        C_statString.text = C_gunModuleReference.S_bulletTraitInformation.e_bulletTrait.ToString() + "<sprite index= " + 7 + ">";
                        C_statString.color = new Color(1f, 0.12f, 0.121f);
                        break;
                }

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
                C_moduleStat2Name.text = "Reload Speed:";
                C_moduleStat4Name.text = "Bullet Element:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(8, 60, 0.1f, 0.9f, C_gunModuleReference.i_clipSize), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(1 / 0.3f, 1 / 3f, 0.1f, 0.9f, 1 / C_gunModuleReference.f_reloadSpeed), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(1 - C_statValueBar2.transform.localScale.x);
                C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString();
                switch (C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect)
                {
                    case GunModule.BulletEffect.None:
                        C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString();
                        C_statString.color = new Color(1f, 1f, 1f);
                        break;
                    case GunModule.BulletEffect.Fire:
                        C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString() + "<sprite index= " + 0 + ">";
                        C_statString.color = new Color(1f, 0.12f, 0.12f);
                        break;
                    case GunModule.BulletEffect.Ice:
                        C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString() + "<sprite index= " + 2 + ">";
                        C_statString.color = new Color(0.32f, 0.85f, 1f);
                        break;
                    case GunModule.BulletEffect.Lightning:
                        C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString() + "<sprite index= " + 1 + ">";
                        C_statString.color = new Color(1f, 0.79f, 0.16f);
                        break;
                    case GunModule.BulletEffect.Vampire:
                        C_statString.text = C_gunModuleReference.S_bulletEffectInformation.e_bulletEffect.ToString() + "<sprite index= " + 3 + ">";
                        C_statString.color = new Color(0.08f, 0.78f, 0.04f);
                        break;
                }
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
                C_moduleStat4Name.text = "Shot Pattern:";
                //Values scale on x axis for horizontal fill, magic numbers at the start of maps are minimum and maximum
                C_statValueBar1.transform.localScale = new Vector3(ExtraMaths.Map(0.2f, 0.85f, 0.1f, 0.9f, C_gunModuleReference.f_bulletSize), 1, 1);
                C_statValueBar1.color = C_statBarGradient.Evaluate(C_statValueBar1.transform.localScale.x);
                C_statValueBar2.transform.localScale = new Vector3(ExtraMaths.Map(2, 8f, 0.1f, 0.9f, C_gunModuleReference.f_bulletRange), 1, 1);
                C_statValueBar2.color = C_statBarGradient.Evaluate(C_statValueBar2.transform.localScale.x);

                switch (C_gunModuleReference.S_shotPatternInformation.e_shotPattern)
                {
                    case GunModule.ShotPattern.Straight:
                        C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString() + "<sprite index= " + 8 + ">";
                        C_statString.color = new Color(0.325f,0.85f,1f);
                        break;
                    case GunModule.ShotPattern.Multishot:
                        C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString() + "<sprite index= " + 10 + ">";
                        C_statString.color = new Color(0.08f, 0.77f, 0.04f);
                        break;
                    case GunModule.ShotPattern.Buckshot:
                        C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString() + "<sprite index= " + 9 + ">";
                        C_statString.color = new Color(1f, 0.83f, 0.09f);
                        break;
                    case GunModule.ShotPattern.Spray:
                        C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString();
                        C_statString.color = new Color(1f, 1f, 1f);
                        break;
                    case GunModule.ShotPattern.Wave:
                        C_statString.text = C_gunModuleReference.S_shotPatternInformation.e_shotPattern.ToString();
                        C_statString.color = new Color(1f, 0.32f, 0.96f);
                        break;
                }
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
