using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class SelectionOutlinePass : CustomRenderPass<SelectionOutlineSettings>
{
    private Material whiteMaterial;
    private Material blitMaterial;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int OutlineTarget = Shader.PropertyToID("_OutlineTarget");

    public override bool Enabled => Settings.active;
    public static List<Renderer> RenderList { get; } = new();

    public SelectionOutlinePass()
    {
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public static void Add(IEnumerable<Renderer> renderers)
    {
        foreach (var r in renderers)
        {
            if (!RenderList.Contains(r)) RenderList.Add(r);
        }
    }

    public static void Remove(IEnumerable<Renderer> renderers)
    {
        foreach (var r in renderers) RenderList.Remove(r);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 32;
        desc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
        cmd.GetTemporaryRT(OutlineTarget, desc, FilterMode.Bilinear);

        if (!whiteMaterial) whiteMaterial = new Material(Shader.Find("Unlit/OutlineObject"));
        if (!blackMaterial) blackMaterial = new Material(Shader.Find("Unlit/OutlineObject"));
        if (!blitMaterial) blitMaterial = new Material(Shader.Find("Hidden/OutlineBlit"));

        RenderList.RemoveAll(e => !e || e.CompareTag("Do Not Outline"));
    }

    public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!whiteMaterial) return;
        if (!blitMaterial) return;

        blitMaterial.SetColor(BaseColor, Settings.outlineColor.value);
        var data = renderingData;

        ExecuteWithCommandBuffer(context, cmd =>
        {
            cmd.SetRenderTarget(OutlineTarget);
            cmd.ClearRenderTarget(true, true, Color.clear);

            foreach (var r in RenderList)
            {
                DrawRenderer(cmd, r);
            }

            cmd.SetRenderTarget(data.cameraData.renderer.cameraColorTarget);
            cmd.DrawProcedural(Matrix4x4.identity, blitMaterial, 0, MeshTopology.Triangles, 3);
        });
    }

    private void DrawRenderer(CommandBuffer cmd, Renderer renderer)
    {
        for (var i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            cmd.DrawRenderer(renderer, whiteMaterial, i, 0);
        }
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
    }
}