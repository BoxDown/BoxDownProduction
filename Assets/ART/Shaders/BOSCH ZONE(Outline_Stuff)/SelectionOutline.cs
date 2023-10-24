using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class SelectionOutline : MonoBehaviour
{
    private IEnumerable<Renderer> renderers;

    private void OnEnable()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SelectionOutlinePass.Add(renderers);
    }

    private void OnDisable()
    {
        SelectionOutlinePass.Remove(renderers);
    }
}