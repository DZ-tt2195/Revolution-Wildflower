using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenuForRenderPipeline("Custom/Half tone bloom", typeof(UniversalRenderPipeline))]
public class HalfToneBloom : VolumeComponent, IPostProcessComponent
{
    [Header("Bloom settings")]
    public FloatParameter threshold = new FloatParameter(0.9f, true);
    public FloatParameter intensity = new FloatParameter(1, true);
    public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0, 1, true);
    public FloatParameter clamp = new FloatParameter(10, true);
    public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 0, 10);
    public NoInterpColorParameter tint = new NoInterpColorParameter(Color.white);

    [Header("Half tone")]
    public IntParameter dotDensity = new IntParameter(10, true);
    public FloatParameter dotCutoff = new FloatParameter(0.4f, true);
    public FloatParameter dotThreshold = new FloatParameter(2.5f, true);
    public FloatParameter bloomPower = new FloatParameter(1f, true);
    
    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
