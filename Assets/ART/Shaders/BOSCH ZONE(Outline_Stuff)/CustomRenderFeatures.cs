using UnityEngine.Rendering.Universal;

public sealed class CustomRenderFeatures : ScriptableRendererFeature
{
    public bool renderOutline = true;

    private SelectionOutlinePass outlinePass;

    public override void Create()
    {
        if (renderOutline) outlinePass = new SelectionOutlinePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderOutline) outlinePass.Enqueue(renderer);
    }
}