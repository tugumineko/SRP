using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer renderer = new CameraRenderer();

    private bool allowHDR;
    private bool useDynamicBatching;
    private bool useGPUInstancing;
    private bool useSRPBatcher;
    private bool useLightsPerObject;
    private ShadowSettings shadowSettings;
    private PostFXSettings postFXSettings;
    private int colorLUTResolution;
    public CustomRenderPipeline(bool allowHDR,bool useDynamicBatching,bool useGPUInstancing,bool useSRPBatcher,bool useLightPerObject,ShadowSettings shadowSettings,PostFXSettings postFXSettings, int colorLUTResolution)
    {
        this.colorLUTResolution = colorLUTResolution;
        this.allowHDR = allowHDR;
        this.shadowSettings = shadowSettings;
        this.postFXSettings = postFXSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.useLightsPerObject = useLightPerObject;
        GraphicsSettings.lightsUseLinearIntensity = true;
        InitializeForEditor();
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {

    }


    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera,allowHDR, useDynamicBatching, useGPUInstancing,useLightsPerObject,shadowSettings,postFXSettings,colorLUTResolution);
        }
    }
}
