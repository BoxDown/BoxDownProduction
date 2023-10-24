using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class CustomRenderPass<T> : ScriptableRenderPass where T : VolumeComponent
{
    public abstract bool Enabled { get; }

    protected T Settings { get; private set; }
    
    public void Enqueue(ScriptableRenderer renderer)
    {
        UpdateVolumeSettings();
        if (!Enabled) return;
        renderer.EnqueuePass(this);
    }

    public sealed override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        UpdateVolumeSettings();

        OnExecute(context, ref renderingData);
    }

    private void UpdateVolumeSettings()
    {
        var stack = VolumeManager.instance.stack;
        Settings = stack.GetComponent<T>();
    }

    public virtual void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        
    }

    protected void ExecuteWithCommandBuffer(ScriptableRenderContext context, Action<CommandBuffer> callback)
    {
        var name = GetType().Name;
        
        var cmd = CommandBufferPool.Get(name);
        cmd.Clear();
        cmd.BeginSample(name);

        using (new ProfilingScope(cmd, new ProfilingSampler(name)))
        {
            callback(cmd);
        }

        cmd.EndSample(name);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
