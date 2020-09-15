using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialPass : ScriptableRenderPass
{
    private Material blitMaterial;
    private int screenCopyId;
    private int screenId;

    public BlitMaterialPass(Material material, string cameraTextureName, string cameraCopyTextureName)
    {
        blitMaterial = material;
        screenId = Shader.PropertyToID(cameraTextureName);
        screenCopyId = Shader.PropertyToID(cameraCopyTextureName);
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var buffer = CommandBufferPool.Get("BlitMaterialPass");

        buffer.GetTemporaryRT(screenCopyId, renderingData.cameraData.cameraTargetDescriptor);
        buffer.SetGlobalTexture(screenId, screenCopyId);
        
        buffer.Blit(colorAttachment, screenCopyId);
        buffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, blitMaterial);
        buffer.ReleaseTemporaryRT(screenCopyId);
        
        context.ExecuteCommandBuffer(buffer);
        CommandBufferPool.Release(buffer);
    }
}
