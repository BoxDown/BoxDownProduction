using System.Collections;
using Utility;
using UnityEngine;
using Managers;
namespace GunkManager
{

    public class PolyBrushManager : MonoBehaviour
    {
        [Rename("Clear Goo Time")] public float f_gooClearTime = 5;
        [HideInInspector] public float f_currentGooValue = 0;
        [Rename("Materials")] public Material[] aC_mat;

        public void InstanceMaterial()
        {
            if (aC_mat.Length != 0)
            {
                for (int i = 0; i < aC_mat.Length; i++)
                {
                    if (aC_mat[i] != null)
                    {
                        aC_mat[i] = new Material(aC_mat[i]);
                    }
                }
            }
        }


        public void RemoveSpaceGoo()
        {
            if (aC_mat != null)
            {
                StartCoroutine(ClearGoo());
            }
        }

        public void ResetGoo()
        {
            if (aC_mat.Length != 0)
            {
                for (int i = 0; i < aC_mat.Length; i++)
                {
                    if (aC_mat[i] != null)
                    {
                        aC_mat[i].SetFloat("_Decay_Amount", 0);
                    }
                }

            }
        }

        private IEnumerator ClearGoo()
        {
            FMOD.Studio.EventInstance clearSound = AudioManager.StartFmodLoop("event:/SFX/Environment/Gunk_Clear");
            float timeClearing = 0;
            while (timeClearing < f_gooClearTime)
            {
                f_currentGooValue = Mathf.Lerp(0, 1, timeClearing / f_gooClearTime);
                timeClearing += Time.deltaTime;
                for (int i = 0; i < aC_mat.Length; i++)
                {
                    if (aC_mat[i] != null)
                    {
                        aC_mat[i].SetFloat("_Decay_Amount", f_currentGooValue);
                    }
                }
                yield return 0;
            }
            AudioManager.EndFmodLoop(clearSound);
        }
    }
}

namespace GunkManager.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(PolyBrushManager))]
    public class PolyBrushManagerEditor : Editor
    {
        PolyBrushManager C_polyBrushManager;
        public override void OnInspectorGUI()
        {
            C_polyBrushManager = (PolyBrushManager)serializedObject.targetObject;
            base.OnInspectorGUI();
            if(GUILayout.Button("Reset Goo - RunTime Only"))
            {
                C_polyBrushManager.ResetGoo();
            }
            if(GUILayout.Button("Clear Goo - RunTime Only"))
            {
                C_polyBrushManager.RemoveSpaceGoo();
            }
        }
    }
#endif

}