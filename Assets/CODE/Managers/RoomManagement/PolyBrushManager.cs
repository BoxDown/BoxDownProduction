using System.Collections;
using Utility;
using UnityEngine;

public class PolyBrushManager : MonoBehaviour
{
    [Rename("Clear Goo Time")] public float f_gooClearTime = 5;
    [HideInInspector] public float f_currentGooValue = 0;
    [Rename("Material")] public Material C_mat;
    public void RemoveSpaceGoo()
    {
        if (C_mat != null)
        {
            StartCoroutine(ClearGoo());
        }
    }

    public void ResetGoo()
    {
        if (C_mat != null)
        {
            C_mat.SetFloat("_Decay_Amount", 0);
        }
    }

    private IEnumerator ClearGoo()
    {
        float timeClearing = 0;
        while (timeClearing < f_gooClearTime)
        {
            f_currentGooValue = Mathf.Lerp(0, 1, timeClearing / f_gooClearTime);
            timeClearing += Time.deltaTime;
            C_mat.SetFloat("_Decay_Amount", f_currentGooValue);
            yield return 0;
        }
    }
}
