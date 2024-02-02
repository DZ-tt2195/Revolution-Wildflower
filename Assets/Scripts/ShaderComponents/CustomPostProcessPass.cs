using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

// Generates a custom post processing pass for URP
// Uses Unity's URP package code with modifications
// Code originally from Game Dev Guide on yt: https://www.youtube.com/watch?v=9fa4uFm1eCE
// I made some changes to make it actually work

[System.Serializable]
public class CustomPostProcessPass : ScriptableRenderPass
{
    RenderTextureDescriptor descriptor;
    RTHandle colorTarget, depthTarget;

    Material bloomMat;
    Material compositeMat;

    HalfToneBloom bloomEffect;

    const int k_MaxPyramidSize = 16;
    private int[] _BloomMipUp;
    private int[] _BloomMipDown;
    private RTHandle[] m_BloomMipUp;
    private RTHandle[] m_BloomMipDown;
    private GraphicsFormat hdrFormat;

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        bloomEffect = stack.GetComponent<HalfToneBloom>();

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Processing")))
        {
            SetupBloom(cmd, colorTarget);

            compositeMat.SetFloat("_Cutoff", bloomEffect.dotCutoff.value);
            compositeMat.SetFloat("_Density", bloomEffect.dotDensity.value);
            compositeMat.SetFloat("_Threshold", bloomEffect.dotThreshold.value);
            compositeMat.SetFloat("_BloomIncrease", bloomEffect.bloomPower.value);
            
            Blitter.BlitCameraTexture(cmd, colorTarget, colorTarget, compositeMat, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        descriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    RenderTextureDescriptor GetCompatibleDescriptor()
            => GetCompatibleDescriptor(descriptor.width, descriptor.height, descriptor.graphicsFormat);

    RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
        => GetCompatibleDescriptor(descriptor, width, height, format, depthBufferBits);

    internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
    {
        desc.depthBufferBits = (int)depthBufferBits;
        desc.msaaSamples = 1;
        desc.width = width;
        desc.height = height;
        desc.graphicsFormat = format;
        return desc;
    }
    internal void SetTarget(RTHandle colorTHandle, RTHandle depthTHandle)
    {
        colorTarget = colorTHandle;
        depthTarget = depthTHandle;
    }

    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
    {
        bloomMat = bloomMaterial;
        compositeMat = compositeMaterial;

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        _BloomMipUp = new int[k_MaxPyramidSize];
        _BloomMipDown = new int[k_MaxPyramidSize];
        m_BloomMipUp = new RTHandle[k_MaxPyramidSize];
        m_BloomMipDown = new RTHandle[k_MaxPyramidSize];

        for (int i = 0; i < k_MaxPyramidSize; i++)
        {
            _BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
            _BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
            m_BloomMipUp[i] = RTHandles.Alloc(_BloomMipUp[i], name: "_BloomMipUp" + i);
            m_BloomMipDown[i] = RTHandles.Alloc(_BloomMipDown[i], name: "_BloomMipDown" + i);
        }

        const FormatUsage usage = FormatUsage.Linear | FormatUsage.Render;
        if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, usage)) //HDR fallback
        {
            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        }
        else
        {
            hdrFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
        }
    }

    private void SetupBloom(CommandBuffer cmd, RTHandle source)
    {
        int downres = 1;
        int tw = descriptor.width >> downres;
        int th = descriptor.height >> downres;

        int maxSize = Mathf.Max(tw, th);
        int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
        int mipCount = Mathf.Clamp(iterations, 1, bloomEffect.maxIterations.value);
        float clamp = bloomEffect.clamp.value;
        float threshold = Mathf.GammaToLinearSpace(bloomEffect.threshold.value);
        float thresholdKnee = threshold * 0.5f;
        float scatter = Mathf.Lerp(0.05f, 0.95f, bloomEffect.scatter.value);

        var bloomMaterial = bloomMat;
        bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));

        var desc = GetCompatibleDescriptor(tw, th, hdrFormat);
        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipUp[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: m_BloomMipUp[i].name);
            RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[i], desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: m_BloomMipDown[i].name);
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }
        Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 0);

        var lastDown = m_BloomMipDown[0];
        for (int i = 1; i < mipCount; i++)
        {
            Blitter.BlitCameraTexture(cmd, lastDown, m_BloomMipUp[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 1);
            Blitter.BlitCameraTexture(cmd, m_BloomMipUp[i], m_BloomMipDown[i], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 2);

            lastDown = m_BloomMipDown[i];
        }

        for(int i = mipCount - 2; i >= 0; i--)
        {
            var lowMip = (i == mipCount - 2) ? m_BloomMipDown[i + 1] : m_BloomMipUp[i + 1];
            var highMip = m_BloomMipDown[i];
            var dst = m_BloomMipUp[i];

            cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
            Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloomMaterial, 3);
        }

        cmd.SetGlobalTexture("_BloomTexture", m_BloomMipUp[0]);
        cmd.SetGlobalFloat("_BloomIntensity", bloomEffect.intensity.value);
    }

    public void Dispose()
    {
        foreach (var handle in m_BloomMipDown)
            handle?.Release();
        foreach (var handle in m_BloomMipUp)
            handle?.Release();
    }
}
