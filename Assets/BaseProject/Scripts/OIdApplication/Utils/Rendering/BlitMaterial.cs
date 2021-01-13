using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlitMaterial : ScriptableRendererFeature
{
    public Material blitMaterial;
    public RenderPassEvent renderPass;
    
    private BlitMaterialPass pass;
    
    public override void Create()
    {
        pass = new BlitMaterialPass(blitMaterial, 
            "_CameraTexture", 
            "_CameraCopyTexture")
        {
            renderPassEvent = renderPass
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
