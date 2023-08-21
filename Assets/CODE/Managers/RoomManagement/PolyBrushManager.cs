using System.Collections;
using Utility;
using UnityEngine;

public class PolyBrushManager : MonoBehaviour
{
    [Rename("Clear Goo Time")] public float f_gooClearTime = 5;
    [HideInInspector]public float f_currentGooValue = 0;
    [Rename("Material")]public Material C_mat;
    public void RemoveSpaceGoo()
    {
        StartCoroutine(ClearGoo());
    }

    public void ResetGoo()
    {
        C_mat.SetFloat("_DECAY_AMOUNT", 0);
    }

    private IEnumerator ClearGoo()
    {
        float timeClearing = 0;
        while(timeClearing < f_gooClearTime)
        {
            f_currentGooValue = Mathf.Lerp(0, 1, timeClearing / f_gooClearTime);
            timeClearing += Time.deltaTime;
            C_mat.SetFloat("_DECAY_AMOUNT", f_currentGooValue * 10);            
            yield return 0;
        }
    }
}
