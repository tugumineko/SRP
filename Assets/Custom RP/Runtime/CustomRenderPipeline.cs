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
    
    public CustomRenderPipeline(CustomRenderPipelineAsset settings)
    {
        this.colorLUTResolution = (int)settings.colorLUTResolution; 
        this.allowHDR = settings.allowHDR;
        this.shadowSettings = settings.shadowSettings;
        this.postFXSettings = settings.postFXSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = settings.useSRPBatcher;
        this.useDynamicBatching = settings.useDynamicBatching;
        this.useGPUInstancing = settings.useGPUInstancing;
        this.useLightsPerObject = settings.useLightsPerObject;
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
