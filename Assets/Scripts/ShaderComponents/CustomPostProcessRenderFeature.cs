using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader bloomShader;
    [SerializeField] private Shader compositeShader;

    private Material bloomMat;
    private Material compositeMat;
    
    private CustomPostProcessPass customPass;
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            customPass.ConfigureInput(ScriptableRenderPassInput.Color);
            customPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }
    public override void Create()
    {
        bloomMat = CoreUtils.CreateEngineMaterial(bloomShader);
        compositeMat = CoreUtils.CreateEngineMaterial(compositeShader);
        
        customPass = new CustomPostProcessPass(bloomMat, compositeMat);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(bloomMat);
        CoreUtils.Destroy(compositeMat);
        customPass.Dispose();
    }
}
