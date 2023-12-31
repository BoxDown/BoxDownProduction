using System;
using System.Collections;
using UnityEngine;
using Utility;
using Managers;

namespace Gun
{
    [CreateAssetMenu(fileName = "GunModule", menuName = "Gun Module")]
    public class GunModule : ScriptableObject
    {
        /// <summary>
        /// Enum setups for further use in variables
        /// </summary>
        #region EnumAndStructSetup
        public enum ModuleSection
        {
            Trigger,
            Clip,
            Barrel,
            Count
        }

        //Trigger Module Enum
        public enum BulletTrait
        {
            Standard,
            Pierce,
            Ricochet,
            Explosive,
            Homing,
            Count
        }

        //Barrel Module Enum
        public enum ShotPattern
        {
            Straight,
            Multishot,
            Buckshot,
            Spray,
            Wave,
            Count
        }

        //Clip Module Enum
        public enum BulletEffect
        {
            None,
            Fire, //Damage over time
            Ice, // Slow then freeze
            Lightning, //chains between enemies
            Vampire, //health steal
            Count
        }

        [Serializable]
        public struct BulletTraitInfo
        {
            //bullet trait deppendent
            [Rename("Bullet Type")] public BulletTrait e_bulletTrait;
            [Rename("Pierce Count")] public int i_pierceCount;
            [Rename("Ricochet Count")] public int i_ricochetCount;
            [Rename("Explosion Prefab")] public GameObject C_explosionPrefab;
            [Rename("Explosion Diameter")] public float f_explosionSize;
            [Rename("Explosion Size Over Lifetime")] public AnimationCurve C_sizeOverLifetimeCurve;
            [Rename("Explosion Damage")] public float f_explosionDamage;
            [Rename("Explosion Knockback Distance")] public float f_explosionKnockbackDistance;
            [Rename("Explosion Life Time")] public float f_explosionLifeTime;
            [Rename("Homing Strength"), Range(0, 1)] public float f_homingStrength;
            [Rename("Homing Delay")] public float f_homingDelayTime;
        }

        [Serializable]
        public struct BulletEffectInfo
        {
            //bullet effect dependent
            [Rename("Bullet Effect")] public BulletEffect e_bulletEffect;
            [Rename("Effect Time")] public float f_effectTime;
            [Rename("Damage Over Time - Damage Per Tick")] public float f_tickDamage;
            [Rename("Damage Over Time - Tick Count")] public int i_amountOfTicks;
            [Rename("Slow Percentage"), Range(0, 1)] public float f_slowPercent;
            [Rename("Electric Radius")] public float f_chainLength;
            [Rename("Electric Damage Percentage"), Range(0, 1)] public float f_chainDamagePercent;
            [Rename("Health Steal Percentage"), Range(0, 1)] public float f_vampirePercent;
        }

        [Serializable]
        public struct ShotPatternInfo
        {
            //shot pattern dependent
            [Rename("Shot Pattern")] public ShotPattern e_shotPattern;
            [Rename("Random Spread")] public bool b_randomSpread;
            [Rename("Shot Count")] public int i_shotCount;
            [Rename("Multi Shot Distance")] public float f_multiShotDistance;
            [Rename("Spread Angle")] public float f_maxAngle;
            [Rename("Wave Speed")] public float f_waveSpeed;
        }
        #endregion

        #region Variables
        //Global
        [Rename("Module Prefab")] public GameObject C_meshPrefab;
        [Rename("Module In Game Name")] public string s_moduleName;
        [Rename("Module Type")] public ModuleSection e_moduleType;
        [Rename("Module Strength"), Range(1, 100)] public float f_moduleStrength;

        //public Gun.FireType e_fireType;

        //Trigger Group
        [Rename("Base Damage")] public float f_baseDamage;
        [Rename("Bullets Fired Per Second")] public float f_fireRate;
        [Rename("Bullet Speed")] public float f_bulletSpeed;
        [Rename("Knock Back")] public float f_knockBack;
        public BulletEffectInfo S_bulletEffectInformation;


        //Clip Group
        [Rename("Reload Time")] public float f_reloadSpeed;
        [Rename("Movement Penalty")] public float f_movementPenalty;
        [Rename("Clip Size")] public int i_clipSize;
        public BulletTraitInfo S_bulletTraitInformation;


        //Barrel Group
        [Rename("Bullet Size")] public float f_bulletSize;
        [Rename("Bullet Range")] public float f_bulletRange;
        [Rename("Recoil Distance")] public float f_recoil;
        public ShotPatternInfo S_shotPatternInformation;
        #endregion

        public void Spawn(Vector3 worldPos)
        {
            GameObject newGunModule = Instantiate(C_meshPrefab);
            newGunModule.transform.position = worldPos;
            newGunModule.transform.rotation = Quaternion.Euler(new Vector3(45,0,0));
            newGunModule.transform.localScale = new Vector3(2.5f,2.5f,2.5f);
            newGunModule.GetComponent<Collider>().isTrigger = true;
            newGunModule.AddComponent<Pickup>();
            switch (e_moduleType)
            {
                case ModuleSection.Trigger:
                    newGunModule.name = $"Trigger\\{name}";
                    GameManager.SetOutlineMaterialColour(new Color(0 / 255f, 255 / 255f, 120 / 255f));
                    break;
                case ModuleSection.Clip:
                    newGunModule.name = $"Clip\\{name}";
                    GameManager.SetOutlineMaterialColour(new Color(255 / 255f, 53 / 255f, 103 / 255f));
                    break;
                case ModuleSection.Barrel:
                    newGunModule.name = $"Barrel\\{name}";
                    GameManager.SetOutlineMaterialColour(new Color(255 / 255f, 115 / 255f, 0 / 255f));
                    break;
            }
            newGunModule.tag = "Gun Module";
            newGunModule.layer = 6;

            newGunModule.transform.Find("GunOutline").gameObject.SetActive(true);
            

        }

    }
}
namespace Guns.CustomEditor
{
    using Gun;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine.Assertions;

    [CustomEditor(typeof(GunModule))]
    class BoxDownGunEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("C_meshPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("s_moduleName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("e_moduleType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("f_moduleStrength"));

            Assert.IsFalse(serializedObject.FindProperty("e_moduleType").enumValueIndex == (int)GunModule.ModuleSection.Count, "A Gun Module cannot have the type 'Count' this is for programming use");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Variables:", EditorStyles.boldLabel);

            switch (serializedObject.FindProperty("e_moduleType").enumValueIndex)
            {
                case 0:
                    //do certain variables yada yada
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_baseDamage"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_fireRate"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_bulletSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_knockBack"));
                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("e_bulletTrait"));

                    Assert.IsFalse(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("e_bulletTrait").enumValueIndex == (int)GunModule.BulletTrait.Count, "A Bullet Trait cannot have the type 'Count' this is for programming use");

                    switch (serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("e_bulletTrait").enumValueIndex)
                    {
                        //do variables for Standard
                        case 0:

                            break;
                        //do variables for Pierce
                        case 1:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("i_pierceCount"));

                            break;

                        case 2:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("i_ricochetCount"));
                            break;
                        //do variables for Explosive
                        case 3:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("C_explosionPrefab"));

                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_explosionDamage"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_explosionSize"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("C_sizeOverLifetimeCurve"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_explosionKnockbackDistance"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_explosionLifeTime"));

                            break;
                        //do variables for Homing
                        case 4:

                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_homingStrength"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletTraitInformation").FindPropertyRelative("f_homingDelayTime"));
                            break;
                    }

                    break;
                case 1:
                    //do certain variables yada yada
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_reloadSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_movementPenalty"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("i_clipSize"));
                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("e_bulletEffect"));

                    Assert.IsFalse(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("e_bulletEffect").enumValueIndex == (int)GunModule.BulletEffect.Count, "A Bullet Effect cannot have the type 'Count' this is for programming use");



                    switch (serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("e_bulletEffect").enumValueIndex)
                    {
                        case 0:
                            break;
                        //do variables for Damage Over Time
                        case 1:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_effectTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_tickDamage"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("i_amountOfTicks"));
                            break;
                        //do variables for Slow
                        case 2:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_effectTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_slowPercent"));
                            break;

                        //do variables for Lightning
                        case 3:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_effectTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_chainLength"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_chainDamagePercent"));
                            break;
                        //do variables for Vampire
                        case 4:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_effectTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_bulletEffectInformation").FindPropertyRelative("f_vampirePercent"));
                            break;
                    }

                    break;
                case 2:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_bulletSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_bulletRange"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("f_recoil"));


                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("e_shotPattern"));
                    serializedObject.ApplyModifiedProperties();
                    Assert.IsFalse(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("e_shotPattern").enumValueIndex == (int)GunModule.ShotPattern.Count, "A Shot Pattern cannot have the type 'Count' this is for programming use");
                    switch (serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("e_shotPattern").enumValueIndex)
                    {
                        case 0:
                            //do variables for Straight


                            break;
                        case 1:
                            //do variables for Multi
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("i_shotCount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("f_multiShotDistance"));

                            break;
                        case 2:
                            //do variables for Buckshot

                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("i_shotCount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("f_maxAngle"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("b_randomSpread"));
                            break;
                        case 3:
                            //do variables for Spray                            
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("f_maxAngle"));

                            break;

                        case 4:
                            //do variables for Wave
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("S_shotPatternInformation").FindPropertyRelative("f_maxAngle"));


                            break;
                    }
                    break;
            }

            // we need to draw the modules assigned varaibles based on enum, then draw certain values after

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}